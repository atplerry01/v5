namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public sealed record Currency
{
    public string Code { get; }
    public string Name { get; }
    public int DecimalPlaces { get; }

    public Currency(string code, string name, int decimalPlaces)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);

        Code = code.ToUpperInvariant();
        Name = name;
        DecimalPlaces = decimalPlaces;
    }

    public Currency(string code) : this(code, code.ToUpperInvariant(), 2) { }

    public static Currency USD => new("USD", "US Dollar", 2);
    public static Currency EUR => new("EUR", "Euro", 2);
    public static Currency GBP => new("GBP", "British Pound", 2);

    public override string ToString() => Code;
}
