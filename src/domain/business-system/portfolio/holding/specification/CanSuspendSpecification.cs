namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(HoldingStatus status)
    {
        return status == HoldingStatus.Active;
    }
}
