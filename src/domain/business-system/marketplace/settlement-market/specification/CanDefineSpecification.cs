namespace Whycespace.Domain.BusinessSystem.Marketplace.SettlementMarket;

public sealed class CanDefineSpecification
{
    public bool IsSatisfiedBy(SettlementMarketStatus status)
    {
        return status == SettlementMarketStatus.Draft;
    }
}
