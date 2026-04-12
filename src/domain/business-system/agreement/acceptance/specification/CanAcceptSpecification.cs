namespace Whycespace.Domain.BusinessSystem.Agreement.Acceptance;

public sealed class CanAcceptSpecification
{
    public bool IsSatisfiedBy(AcceptanceStatus status)
    {
        return status == AcceptanceStatus.Pending;
    }
}
