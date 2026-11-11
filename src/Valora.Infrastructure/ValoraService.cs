using Microsoft.EntityFrameworkCore;
using Npgsql;
using Valora.Application;
using Valora.Domain;

namespace Valora.Infrastructure;

public sealed class ValoraService(ValoraDbContext db, IMonthClock clock, IMonthSummaryCalculator summaryCalculator) : IValoraService
{
    public async Task<MonthResponse> GetMonthAsync(string monthText, bool oneOff, CancellationToken cancellationToken)
    {
        var month = ParseMonth(monthText);
        var query = db.Entries.AsNoTracking().Where(entry => entry.Month == month);
        if (oneOff)
            query = query.Where(entry => entry.RecurrenceRuleId == null);
        var entries = await query.OrderBy(entry => entry.Kind).ThenBy(entry => entry.Description)
            .ToListAsync(cancellationToken);
        return BuildMonthResponse(month, entries);
    }

    public async Task<MonthResponse> OpenMonthAsync(string monthText, CancellationToken cancellationToken)
    {
        var month = ParseMonth(monthText);
        var current = clock.CurrentMonth;

        try
        {
            await InTransactionAsync(async () =>
            {
                var rules = await db.RecurrenceRules.Include(rule => rule.Versions).ToListAsync(cancellationToken);
                var existingRuleIds = (await db.Entries.Where(entry => entry.Month == month && entry.RecurrenceRuleId != null)
                    .Select(entry => entry.RecurrenceRuleId!.Value).ToListAsync(cancellationToken)).ToHashSet();

                foreach (var rule in rules)
                {
                    if (month.CompareTo(rule.StartMonth) < 0 ||
                        (rule.EndMonth.HasValue && month.CompareTo(rule.EndMonth.Value) > 0) ||
                        existingRuleIds.Contains(rule.Id))
                        continue;

                    var version = rule.VersionFor(month);
                    if (version is null || version.Status != RecurrenceStatus.Active)
                        continue;

                    db.Entries.Add(CreateRecurringEntry(rule, version, month));
                }

                await db.SaveChangesAsync(cancellationToken);
                return true;
            }, cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            // Another concurrent month-open request won the unique (rule, month) insert.
            db.ChangeTracker.Clear();
        }

        var entries = await db.Entries.AsNoTracking().Where(entry => entry.Month == month)
            .OrderBy(entry => entry.Kind).ThenBy(entry => entry.Description).ToListAsync(cancellationToken);
        return BuildMonthResponse(month, entries);
    }

    public async Task<EntryResponse> CreateEntryAsync(string monthText, CreateEntryRequest request,
        CancellationToken cancellationToken)
    {
        var month = ParseMonth(monthText);
        EnsureEditable(month);
        var kind = ParseKind(request.Kind);
        var amount = ParseAmount(request.Amount, allowNull: false)!.Value;
        var entry = new Entry(Guid.NewGuid(), month, kind, ParseDescription(request.Description), amount,
            EntryStatus.Complete, null, null, ParseNote(request.Note));
        db.Entries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(entry);
    }

    public async Task<EntryResponse> UpdateEntryAsync(string monthText, Guid entryId, UpdateEntryRequest request,
        CancellationToken cancellationToken)
    {
        var month = ParseMonth(monthText);
        EnsureEditable(month);
        var entry = await db.Entries.SingleOrDefaultAsync(candidate => candidate.Id == entryId && candidate.Month == month,
            cancellationToken) ?? throw NotFound("entry_not_found", "Entry was not found in that month.");
        entry.Update(ParseAmount(request.Amount, allowNull: false), ParseDescription(request.Description), ParseNote(request.Note));
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(entry);
    }

    public async Task<RecurrenceRuleResponse> CreateRuleAsync(RecurrenceRequest request,
        CancellationToken cancellationToken)
    {
        var effective = ParseMonth(request.EffectiveFromMonth);
        EnsureEditable(effective);
        var kind = ParseKind(request.Kind);
        var (mode, amount) = ParseValueMode(request.ValueMode, request.DefaultAmount);
        var description = ParseDescription(request.Description);
        var rule = new RecurrenceRule(Guid.NewGuid(), kind, description, effective);
        var version = new RecurrenceRuleVersion(Guid.NewGuid(), rule.Id, effective, kind, description,
            RecurrenceStatus.Active, mode, amount);
        rule.AddVersion(version);
        var initialEntry = CreateRecurringEntry(rule, version, effective);

        await InTransactionAsync(async () =>
        {
            db.RecurrenceRules.Add(rule);
            db.Entries.Add(initialEntry);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);

        return ToResponse(rule, version);
    }

    public async Task<RecurrenceRuleResponse> UpdateRuleAsync(Guid ruleId, RecurrenceRequest request,
        CancellationToken cancellationToken)
    {
        var effective = ParseMonth(request.EffectiveFromMonth);
        EnsureEditable(effective);
        var rule = await db.RecurrenceRules.Include(candidate => candidate.Versions)
            .SingleOrDefaultAsync(candidate => candidate.Id == ruleId, cancellationToken)
            ?? throw NotFound("rule_not_found", "Recurrence rule was not found.");
        if (effective.CompareTo(rule.StartMonth) < 0)
            throw Conflict("recurrence_conflict", "A recurrence cannot be changed before it starts.");
        var previous = rule.VersionFor(effective);
        if (previous is null || previous.Status == RecurrenceStatus.Ended)
            throw Conflict("recurrence_conflict", "An ended recurrence cannot be changed or resumed.");
        var kind = ParseKind(request.Kind);
        var description = ParseDescription(request.Description);
        var (mode, amount) = ParseValueMode(request.ValueMode, request.DefaultAmount);
        var version = new RecurrenceRuleVersion(Guid.NewGuid(), rule.Id, effective, kind, description,
            previous.Status, mode, amount);
        try
        {
            rule.AddVersion(version);
        }
        catch (InvalidOperationException exception)
        {
            throw Conflict("recurrence_conflict", exception.Message);
        }
        db.RecurrenceRuleVersions.Add(version);

        var mutableEntries = await db.Entries.Where(entry => entry.RecurrenceRuleId == rule.Id)
            .ToListAsync(cancellationToken);
        foreach (var entry in mutableEntries.Where(entry => entry.Month.CompareTo(effective) >= 0 &&
                     entry.Month.CompareTo(clock.CurrentMonth) >= 0))
            entry.ApplyRuleVersion(version);

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(rule, version);
    }

    public async Task<RecurrenceRuleResponse> TransitionRuleAsync(Guid ruleId, string action,
        RecurrenceTransitionRequest request, CancellationToken cancellationToken)
    {
        var effective = ParseMonth(request.EffectiveFromMonth);
        EnsureEditable(effective);
        var rule = await db.RecurrenceRules.Include(candidate => candidate.Versions)
            .SingleOrDefaultAsync(candidate => candidate.Id == ruleId, cancellationToken)
            ?? throw NotFound("rule_not_found", "Recurrence rule was not found.");
        var previous = rule.VersionFor(effective);
        if (previous is null)
            throw Conflict("recurrence_conflict", "No recurrence configuration exists for that month.");

        var status = action switch
        {
            "pause" when previous.Status == RecurrenceStatus.Active => RecurrenceStatus.Paused,
            "resume" when previous.Status == RecurrenceStatus.Paused => RecurrenceStatus.Active,
            "end" when previous.Status != RecurrenceStatus.Ended => RecurrenceStatus.Ended,
            _ => throw Conflict("recurrence_conflict", "The requested recurrence transition is not valid.")
        };
        var version = new RecurrenceRuleVersion(Guid.NewGuid(), rule.Id, effective, previous.Kind,
            previous.Description, status, previous.ValueMode, previous.DefaultAmount);
        try
        {
            rule.AddVersion(version);
        }
        catch (InvalidOperationException exception)
        {
            throw Conflict("recurrence_conflict", exception.Message);
        }
        db.RecurrenceRuleVersions.Add(version);

        if (action == "end")
            rule.SetEndMonth(PreviousMonth(effective));
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(rule, version);
    }

    private async Task<T> InTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
            return await action();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Entry CreateRecurringEntry(RecurrenceRule rule, RecurrenceRuleVersion version, MonthKey month)
    {
        var amount = version.ValueMode == RecurrenceValueMode.CopyDefault ? version.DefaultAmount : null;
        return new Entry(Guid.NewGuid(), month, version.Kind, version.Description, amount,
            amount.HasValue ? EntryStatus.Complete : EntryStatus.PendingAmount,
            rule.Id, version.Id, null);
    }

    private MonthResponse BuildMonthResponse(MonthKey month, IReadOnlyList<Entry> entries)
    {
        var summary = summaryCalculator.Calculate(entries);
        return new MonthResponse(month.ToString(), month.CompareTo(clock.CurrentMonth) < 0,
            entries.Select(ToResponse).ToArray(), new MonthSummaryResponse(Format(summary.IncomeTotal),
                Format(summary.ExpenseTotal), Format(summary.Balance), summary.PendingAmountCount));
    }

    private static EntryResponse ToResponse(Entry entry) => new(entry.Id, entry.Kind.ToString().ToLowerInvariant(),
        entry.Description, entry.Amount?.ToString(), entry.Status == EntryStatus.PendingAmount ? "pending_amount" : "complete",
        entry.Month.ToString(), entry.RecurrenceRuleId, entry.Note, entry.IsRecurring);

    private static RecurrenceRuleResponse ToResponse(RecurrenceRule rule, RecurrenceRuleVersion version) =>
        new(rule.Id, version.Kind.ToString().ToLowerInvariant(), version.Description, rule.StartMonth.ToString(),
            rule.EndMonth?.ToString(), version.Status.ToString().ToLowerInvariant(),
            version.ValueMode == RecurrenceValueMode.CopyDefault ? "copy_default" : "leave_pending",
            version.DefaultAmount?.ToString());

    private static MonthKey ParseMonth(string? value) => MonthKey.TryParse(value, out var month)
        ? month
        : throw new ValoraException(400, "invalid_request", "Month must use the YYYY-MM format.");

    private static EntryKind ParseKind(string? value) => value?.ToLowerInvariant() switch
    {
        "income" => EntryKind.Income,
        "expense" => EntryKind.Expense,
        _ => throw new ValoraException(400, "invalid_request", "Kind must be income or expense.")
    };

    private static string ParseDescription(string? value)
    {
        var description = value?.Trim();
        if (string.IsNullOrWhiteSpace(description) || description.Length > 300)
            throw new ValoraException(400, "invalid_request", "Description must contain 1 to 300 characters.");
        return description;
    }

    private static string? ParseNote(string? value)
    {
        var note = value?.Trim();
        if (note?.Length > 2000)
            throw new ValoraException(400, "invalid_request", "Note must not exceed 2000 characters.");
        return string.IsNullOrEmpty(note) ? null : note;
    }

    private static MoneyAmount? ParseAmount(string? value, bool allowNull)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (allowNull)
                return null;
            throw new ValoraException(400, "invalid_request", "Amount is required.");
        }
        if (!MoneyAmount.TryParse(value, out var amount))
            throw new ValoraException(400, "invalid_request", "Amount must be non-negative and have at most two decimal places.");
        return amount;
    }

    private static (RecurrenceValueMode Mode, MoneyAmount? Amount) ParseValueMode(string? valueMode, string? defaultAmount) =>
        valueMode?.ToLowerInvariant() switch
        {
            "copy_default" => (RecurrenceValueMode.CopyDefault, ParseAmount(defaultAmount, allowNull: false)),
            "leave_pending" when string.IsNullOrWhiteSpace(defaultAmount) => (RecurrenceValueMode.LeavePending, null),
            "leave_pending" => throw new ValoraException(400, "invalid_request", "defaultAmount must be omitted for leave_pending."),
            _ => throw new ValoraException(400, "invalid_request", "valueMode must be copy_default or leave_pending.")
        };

    private void EnsureEditable(MonthKey month)
    {
        if (month.CompareTo(clock.CurrentMonth) < 0)
            throw Conflict("month_read_only", "Months before the current month are read-only.");
    }

    private static MonthKey PreviousMonth(MonthKey month) => month.Month == 1
        ? new MonthKey(Math.Max(1, month.Year - 1), 12)
        : new MonthKey(month.Year, month.Month - 1);

    private static string Format(decimal value) => value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
    private static ValoraException NotFound(string code, string message) => new(404, code, message);
    private static ValoraException Conflict(string code, string message) => new(409, code, message);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
