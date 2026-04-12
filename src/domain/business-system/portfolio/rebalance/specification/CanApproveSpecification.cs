namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public sealed class CanApproveSpecification
{
    public bool IsSatisfiedBy(RebalanceStatus status)
    {
        return status == RebalanceStatus.Pending;
    }
}
