using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public sealed record MoneyAmount : ValueObject
{
    public decimal Amount { get; }
    public string CurrencyCode { get; }

    private MoneyAmount(decimal amount, string currency)
    {
        Amount = amount;
        CurrencyCode = currency;
    }

    public static MoneyAmount Create(decimal amount, string currency)
    {
        if (amount < 0m) throw PricingErrors.InvalidAmount();
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw PricingErrors.InvalidCurrency();
        return new MoneyAmount(amount, currency.Trim().ToUpperInvariant());
    }

    public override string ToString() => $"{Amount:0.##} {CurrencyCode}";
}
