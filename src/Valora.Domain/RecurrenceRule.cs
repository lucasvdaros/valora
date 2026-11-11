namespace Valora.Domain;

public enum RecurrenceStatus
{
    Active,
    Paused,
    Ended
}

public enum RecurrenceValueMode
{
    CopyDefault,
    LeavePending
}

public sealed class RecurrenceRule
{
    private RecurrenceRule() { }

    public RecurrenceRule(Guid id, EntryKind kind, string description, MonthKey startMonth)
    {
        Id = id;
        Kind = kind;
        Description = NormalizeDescription(description);
        StartMonth = startMonth;
    }

    public Guid Id { get; private set; }
    public EntryKind Kind { get; private set; }
    public string Description { get; private set; } = "";
    public MonthKey StartMonth { get; private set; }
    public MonthKey? EndMonth { get; private set; }
    public List<RecurrenceRuleVersion> Versions { get; private set; } = [];

    public void AddVersion(RecurrenceRuleVersion version)
    {
        if (Versions.Any(existing => existing.EffectiveFromMonth == version.EffectiveFromMonth))
            throw new InvalidOperationException("A recurrence version already exists for that effective month.");
        if (version.EffectiveFromMonth.CompareTo(StartMonth) < 0)
            throw new InvalidOperationException("A recurrence version cannot predate the rule.");

        Versions.Add(version);
    }

    public void ChangeDefinition(EntryKind kind, string description)
    {
        Kind = kind;
        Description = NormalizeDescription(description);
    }

    public void SetEndMonth(MonthKey? endMonth) => EndMonth = endMonth;

    public RecurrenceRuleVersion? VersionFor(MonthKey month) =>
        Versions.Where(version => version.EffectiveFromMonth.CompareTo(month) <= 0)
            .MaxBy(version => version.EffectiveFromMonth);

    private static string NormalizeDescription(string description)
    {
        var normalized = description?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Description is required.", nameof(description));
        return normalized;
    }
}

public sealed class RecurrenceRuleVersion
{
    private RecurrenceRuleVersion() { }

    public RecurrenceRuleVersion(Guid id, Guid ruleId, MonthKey effectiveFromMonth,
        EntryKind kind, string description, RecurrenceStatus status, RecurrenceValueMode valueMode,
        MoneyAmount? defaultAmount)
    {
        if ((valueMode == RecurrenceValueMode.CopyDefault) != defaultAmount.HasValue)
            throw new ArgumentException("copy_default requires an amount; leave_pending must not have one.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        Id = id;
        RuleId = ruleId;
        EffectiveFromMonth = effectiveFromMonth;
        Kind = kind;
        Description = description.Trim();
        Status = status;
        ValueMode = valueMode;
        DefaultAmount = defaultAmount;
    }

    public Guid Id { get; private set; }
    public Guid RuleId { get; private set; }
    public MonthKey EffectiveFromMonth { get; private set; }
    public EntryKind Kind { get; private set; }
    public string Description { get; private set; } = "";
    public RecurrenceStatus Status { get; private set; }
    public RecurrenceValueMode ValueMode { get; private set; }
    public MoneyAmount? DefaultAmount { get; private set; }
}
