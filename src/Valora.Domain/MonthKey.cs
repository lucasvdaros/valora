using System.Globalization;

namespace Valora.Domain;

public readonly record struct MonthKey : IComparable<MonthKey>
{
    public int Year { get; }
    public int Month { get; }

    public MonthKey(int year, int month)
    {
        if (year is < 1 or > 9999)
            throw new ArgumentOutOfRangeException(nameof(year));
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month));

        Year = year;
        Month = month;
    }

    public static bool TryParse(string? value, out MonthKey month)
    {
        month = default;
        if (value is null || value.Length != 7 || value[4] != '-' ||
            !int.TryParse(value.AsSpan(0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var year) ||
            !int.TryParse(value.AsSpan(5, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var number) ||
            year is < 1 or > 9999 || number is < 1 or > 12)
            return false;

        month = new MonthKey(year, number);
        return true;
    }

    public int CompareTo(MonthKey other)
    {
        var yearComparison = Year.CompareTo(other.Year);
        return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
    }

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
