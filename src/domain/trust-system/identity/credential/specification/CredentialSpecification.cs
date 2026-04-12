namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(CredentialStatus status)
    {
        return status == CredentialStatus.Issued;
    }
}

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(CredentialStatus status)
    {
        return status == CredentialStatus.Active;
    }
}
