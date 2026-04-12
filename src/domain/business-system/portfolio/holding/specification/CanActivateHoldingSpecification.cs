namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public sealed class CanActivateHoldingSpecification
{
    public bool IsSatisfiedBy(HoldingStatus status)
    {
        return status == HoldingStatus.Opened;
    }
}
