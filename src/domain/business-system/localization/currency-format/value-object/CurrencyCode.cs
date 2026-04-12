namespace Whycespace.Domain.BusinessSystem.Localization.CurrencyFormat;

public readonly record struct CurrencyCode
{
    public string Code { get; }
    public string Symbol { get; }
    public int DecimalPlaces { get; }

    public CurrencyCode(string code, string symbol, int decimalPlaces)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code must not be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Currency symbol must not be empty.", nameof(symbol));

        if (decimalPlaces < 0)
            throw new ArgumentException("Decimal places must not be negative.", nameof(decimalPlaces));

        Code = code;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }
}
