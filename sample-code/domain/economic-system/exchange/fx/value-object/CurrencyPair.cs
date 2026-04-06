namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

/// <summary>
/// Immutable currency pair (e.g., USD/EUR).
/// BaseCurrency is what you're selling, QuoteCurrency is what you're buying.
/// </summary>
public sealed record CurrencyPair
{
    public string BaseCurrency { get; }
    public string QuoteCurrency { get; }

    private CurrencyPair(string baseCurrency, string quoteCurrency)
    {
        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
    }

    public static CurrencyPair From(string baseCurrency, string quoteCurrency)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency))
            throw new ArgumentException("Base currency is required.");
        if (string.IsNullOrWhiteSpace(quoteCurrency))
            throw new ArgumentException("Quote currency is required.");
        if (string.Equals(baseCurrency, quoteCurrency, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Base and quote currencies must differ.");
        return new(baseCurrency.ToUpperInvariant(), quoteCurrency.ToUpperInvariant());
    }

    public CurrencyPair Invert() => new(QuoteCurrency, BaseCurrency);

    public override string ToString() => $"{BaseCurrency}/{QuoteCurrency}";
}
