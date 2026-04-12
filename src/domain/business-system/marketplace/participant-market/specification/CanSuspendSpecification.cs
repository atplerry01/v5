namespace Whycespace.Domain.BusinessSystem.Marketplace.ParticipantMarket;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(ParticipantMarketStatus status)
    {
        return status == ParticipantMarketStatus.Active;
    }
}
