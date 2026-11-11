using Valora.Domain;

namespace Valora.Application;

public sealed record MonthSummary(decimal IncomeTotal, decimal ExpenseTotal, decimal Balance, int PendingAmountCount);

public sealed record MonthView(MonthKey Month, bool IsReadOnly, IReadOnlyList<Entry> Entries, MonthSummary Summary);

public interface IMonthSummaryCalculator
{
    MonthSummary Calculate(IEnumerable<Entry> entries);
}

public sealed class MonthSummaryCalculator : IMonthSummaryCalculator
{
    public MonthSummary Calculate(IEnumerable<Entry> entries)
    {
        decimal income = 0m;
        decimal expenses = 0m;
        var pending = 0;

        foreach (var entry in entries)
        {
            if (!entry.Amount.HasValue)
            {
                pending++;
                continue;
            }

            if (entry.Kind == EntryKind.Income)
                income += entry.Amount.Value.Value;
            else
                expenses += entry.Amount.Value.Value;
        }

        return new MonthSummary(income, expenses, income - expenses, pending);
    }
}

public interface IMonthClock
{
    MonthKey CurrentMonth { get; }
}
