namespace Whycespace.Domain.BusinessSystem.Marketplace.SettlementMarket;

public readonly record struct SettlementTerms
{
    public string SettlementType { get; }
    public string Currency { get; }

    public SettlementTerms(string settlementType, string currency)
    {
        if (string.IsNullOrWhiteSpace(settlementType))
            throw new ArgumentException("Settlement type must not be empty.", nameof(settlementType));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Settlement currency must not be empty.", nameof(currency));

        SettlementType = settlementType;
        Currency = currency;
    }
}
