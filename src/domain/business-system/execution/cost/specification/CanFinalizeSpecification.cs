namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public sealed class CanFinalizeSpecification
{
    public bool IsSatisfiedBy(CostStatus status)
    {
        return status == CostStatus.Calculated;
    }
}
