namespace Whycespace.Domain.BusinessSystem.Billing.Adjustment;

public sealed class CanApplyAdjustmentSpecification
{
    public bool IsSatisfiedBy(AdjustmentStatus status)
    {
        return status == AdjustmentStatus.Draft;
    }
}
