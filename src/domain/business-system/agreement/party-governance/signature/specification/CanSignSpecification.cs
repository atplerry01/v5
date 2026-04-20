namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;

public sealed class CanSignSpecification
{
    public bool IsSatisfiedBy(SignatureStatus status)
    {
        return status == SignatureStatus.Pending;
    }
}
