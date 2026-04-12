namespace Whycespace.Domain.BusinessSystem.Agreement.Signature;

public sealed class CanSignSpecification
{
    public bool IsSatisfiedBy(SignatureStatus status)
    {
        return status == SignatureStatus.Pending;
    }
}
