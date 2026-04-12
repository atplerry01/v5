namespace Whycespace.Domain.BusinessSystem.Marketplace.SettlementMarket;

public sealed class CanSealSpecification
{
    public bool IsSatisfiedBy(SettlementMarketStatus status)
    {
        return status == SettlementMarketStatus.Defined;
    }
}
