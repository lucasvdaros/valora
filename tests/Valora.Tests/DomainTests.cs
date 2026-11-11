using Valora.Application;
using Valora.Domain;

namespace Valora.Tests;

public sealed class DomainTests
{
    [Theory]
    [InlineData("2026-11", true)]
    [InlineData("2026-1", false)]
    [InlineData("2026-13", false)]
    [InlineData("2026-00", false)]
    [InlineData("202611", false)]
    public void MonthKey_ParsesOnlyCivilYearMonth(string value, bool expected)
    {
        Assert.Equal(expected, MonthKey.TryParse(value, out _));
    }

    [Fact]
    public void MoneyAmount_RejectsNegativeAndMoreThanTwoDecimalPlaces()
    {
        Assert.False(MoneyAmount.TryParse("-1.00", out _));
        Assert.False(MoneyAmount.TryParse("1.001", out _));
        Assert.True(MoneyAmount.TryParse("1234.50", out var amount));
        Assert.Equal("1234.50", amount.ToString());
    }

    [Fact]
    public void Summary_ExcludesPendingAmountsFromTotalsAndBalance()
    {
        var month = new MonthKey(2026, 11);
        var entries = new[]
        {
            new Entry(Guid.NewGuid(), month, EntryKind.Income, "Salary", new MoneyAmount(5000m),
                EntryStatus.Complete, null, null, null),
            new Entry(Guid.NewGuid(), month, EntryKind.Expense, "Rent", new MoneyAmount(1800m),
                EntryStatus.Complete, null, null, null),
            new Entry(Guid.NewGuid(), month, EntryKind.Expense, "Variable bill", null,
                EntryStatus.PendingAmount, Guid.NewGuid(), Guid.NewGuid(), null)
        };

        var summary = new MonthSummaryCalculator().Calculate(entries);

        Assert.Equal(5000m, summary.IncomeTotal);
        Assert.Equal(1800m, summary.ExpenseTotal);
        Assert.Equal(3200m, summary.Balance);
        Assert.Equal(1, summary.PendingAmountCount);
    }

    [Fact]
    public void RecurrenceRule_ResolvesTheVersionEffectiveForRequestedMonth()
    {
        var id = Guid.NewGuid();
        var start = new MonthKey(2026, 11);
        var rule = new RecurrenceRule(id, EntryKind.Expense, "Rent", start);
        var original = new RecurrenceRuleVersion(Guid.NewGuid(), id, start, EntryKind.Expense, "Rent",
            RecurrenceStatus.Active, RecurrenceValueMode.CopyDefault, new MoneyAmount(1800m));
        var changed = new RecurrenceRuleVersion(Guid.NewGuid(), id, new MonthKey(2027, 1), EntryKind.Expense, "New rent",
            RecurrenceStatus.Active, RecurrenceValueMode.CopyDefault, new MoneyAmount(1900m));
        rule.AddVersion(original);
        rule.AddVersion(changed);

        Assert.Equal(original.Id, rule.VersionFor(new MonthKey(2026, 12))!.Id);
        Assert.Equal(changed.Id, rule.VersionFor(new MonthKey(2027, 1))!.Id);
    }
}
