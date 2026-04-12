namespace Whycespace.Domain.BusinessSystem.Marketplace.ParticipantMarket;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ParticipantMarketStatus status)
    {
        return status == ParticipantMarketStatus.Registered
            || status == ParticipantMarketStatus.Suspended;
    }
}
