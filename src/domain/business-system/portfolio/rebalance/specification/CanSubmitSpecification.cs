namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public sealed class CanSubmitSpecification
{
    public bool IsSatisfiedBy(RebalanceStatus status)
    {
        return status == RebalanceStatus.Draft
            || status == RebalanceStatus.Rejected;
    }
}
