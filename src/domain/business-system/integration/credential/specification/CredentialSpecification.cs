namespace Whycespace.Domain.BusinessSystem.Integration.Credential;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(CredentialStatus status)
    {
        return status == CredentialStatus.Registered;
    }
}

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(CredentialStatus status)
    {
        return status == CredentialStatus.Active;
    }
}
