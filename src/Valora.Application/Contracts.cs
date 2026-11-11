namespace Valora.Application;

public sealed record EntryResponse(Guid Id, string Kind, string Description, string? Amount, string Status,
    string Month, Guid? RecurrenceRuleId, string? Note, bool IsRecurring);

public sealed record MonthSummaryResponse(string IncomeTotal, string ExpenseTotal, string Balance, int PendingAmountCount);

public sealed record MonthResponse(string Month, bool IsReadOnly, IReadOnlyList<EntryResponse> Entries,
    MonthSummaryResponse Summary);

public sealed record RecurrenceRuleResponse(Guid Id, string Kind, string Description, string StartMonth,
    string? EndMonth, string Status, string ValueMode, string? DefaultAmount);

public sealed record CreateEntryRequest(string Kind, string Description, string Amount, string? Note);
public sealed record UpdateEntryRequest(string Description, string Amount, string? Note);
public sealed record RecurrenceRequest(string Kind, string Description, string EffectiveFromMonth,
    string ValueMode, string? DefaultAmount);
public sealed record RecurrenceTransitionRequest(string EffectiveFromMonth);

public interface IValoraService
{
    Task<MonthResponse> GetMonthAsync(string month, bool oneOff, CancellationToken cancellationToken);
    Task<MonthResponse> OpenMonthAsync(string month, CancellationToken cancellationToken);
    Task<EntryResponse> CreateEntryAsync(string month, CreateEntryRequest request, CancellationToken cancellationToken);
    Task<EntryResponse> UpdateEntryAsync(string month, Guid entryId, UpdateEntryRequest request, CancellationToken cancellationToken);
    Task<RecurrenceRuleResponse> CreateRuleAsync(RecurrenceRequest request, CancellationToken cancellationToken);
    Task<RecurrenceRuleResponse> UpdateRuleAsync(Guid ruleId, RecurrenceRequest request, CancellationToken cancellationToken);
    Task<RecurrenceRuleResponse> TransitionRuleAsync(Guid ruleId, string action, RecurrenceTransitionRequest request, CancellationToken cancellationToken);
}

public sealed class ValoraException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}
