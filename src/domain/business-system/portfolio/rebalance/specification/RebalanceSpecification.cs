namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public sealed class RebalanceSpecification
{
    public bool IsSatisfiedBy(RebalanceId id, RebalanceName name)
    {
        return id.Value != Guid.Empty
            && !string.IsNullOrWhiteSpace(name.Value);
    }
}
