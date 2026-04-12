namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public sealed class CanCloseHoldingSpecification
{
    public bool IsSatisfiedBy(HoldingStatus status)
    {
        return status == HoldingStatus.Suspended;
    }
}
