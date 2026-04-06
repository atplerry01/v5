namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CredentialActiveSpecification
{
    public bool IsSatisfiedBy(CredentialStatus status) => status == CredentialStatus.Active;
}
