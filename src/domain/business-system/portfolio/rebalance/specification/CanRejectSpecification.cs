namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(RebalanceStatus status)
    {
        return status == RebalanceStatus.Pending;
    }
}
