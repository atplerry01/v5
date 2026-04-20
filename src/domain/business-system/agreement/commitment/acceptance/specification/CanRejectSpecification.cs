namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(AcceptanceStatus status)
    {
        return status == AcceptanceStatus.Pending;
    }
}
