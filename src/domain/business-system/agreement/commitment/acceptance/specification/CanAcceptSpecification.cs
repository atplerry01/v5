namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public sealed class CanAcceptSpecification
{
    public bool IsSatisfiedBy(AcceptanceStatus status)
    {
        return status == AcceptanceStatus.Pending;
    }
}
