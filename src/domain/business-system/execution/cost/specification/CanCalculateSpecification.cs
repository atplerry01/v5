namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public sealed class CanCalculateSpecification
{
    public bool IsSatisfiedBy(CostStatus status)
    {
        return status == CostStatus.Pending;
    }
}
