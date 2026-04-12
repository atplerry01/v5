namespace Whycespace.Domain.BusinessSystem.Billing.Adjustment;

public sealed class CanVoidAdjustmentSpecification
{
    public bool IsSatisfiedBy(AdjustmentStatus status)
    {
        return status == AdjustmentStatus.Applied;
    }
}
