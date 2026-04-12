namespace Whycespace.Domain.BusinessSystem.Agreement.Acceptance;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(AcceptanceStatus status)
    {
        return status == AcceptanceStatus.Pending;
    }
}
