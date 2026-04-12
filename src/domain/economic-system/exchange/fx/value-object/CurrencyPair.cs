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

        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
    }
}
