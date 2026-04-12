namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public sealed class IsFinalizedSpecification
{
    public bool IsSatisfiedBy(CostStatus status)
    {
        return status == CostStatus.Finalized;
    }
}
