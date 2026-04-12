namespace Whycespace.Domain.BusinessSystem.Agreement.Signature;

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(SignatureStatus status)
    {
        return status == SignatureStatus.Signed;
    }
}
