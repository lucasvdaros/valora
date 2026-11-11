namespace Valora.Domain;

public readonly record struct MoneyAmount
{
    public decimal Value { get; }

    public MoneyAmount(decimal value)
    {
        if (value < 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Negative amounts are not allowed.");

        var scale = (decimal.GetBits(value)[3] >> 16) & 0x7F;
        if (scale > 2)
            throw new ArgumentOutOfRangeException(nameof(value), "Amounts may have at most two decimal places.");

        Value = value;
    }

    public static MoneyAmount Zero => new(0m);

    public static bool TryParse(string? value, out MoneyAmount amount)
    {
        amount = Zero;
        if (!decimal.TryParse(value, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return false;

        try
        {
            amount = new MoneyAmount(parsed);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public override string ToString() => Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
}
