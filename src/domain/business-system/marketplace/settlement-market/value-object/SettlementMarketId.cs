namespace Whycespace.Domain.BusinessSystem.Marketplace.SettlementMarket;

public readonly record struct SettlementMarketId
{
    public Guid Value { get; }

    public SettlementMarketId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SettlementMarketId value must not be empty.", nameof(value));

        Value = value;
    }
}
