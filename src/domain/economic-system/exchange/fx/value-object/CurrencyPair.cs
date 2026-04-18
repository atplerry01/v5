using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public readonly record struct CurrencyPair
{
    public Currency BaseCurrency { get; }
    public Currency QuoteCurrency { get; }

    public CurrencyPair(Currency baseCurrency, Currency quoteCurrency)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency.Code))
            throw new ArgumentException("Base currency code must not be empty.", nameof(baseCurrency));

        if (string.IsNullOrWhiteSpace(quoteCurrency.Code))
            throw new ArgumentException("Quote currency code must not be empty.", nameof(quoteCurrency));

        // Phase 6 T6.2 — canonical ordering invariant. Base MUST precede
        // Quote ordinal. Guarantees one row per economic pair (USD/EUR is
        // stored, EUR/USD is not). Without this, the rate resolver can
        // non-deterministically resolve either orientation for the same
        // pair and downstream ledger posting becomes ambiguous.
        if (string.CompareOrdinal(baseCurrency.Code, quoteCurrency.Code) >= 0)
            throw new ArgumentException(
                $"CurrencyPair invariant violated: Base ('{baseCurrency.Code}') must precede Quote " +
                $"('{quoteCurrency.Code}') ordinally. Use the inverse pair and invert the rate instead.",
                nameof(baseCurrency));

        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
    }
}
