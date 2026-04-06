namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CredentialNotExpiredSpecification
{
    public bool IsSatisfiedBy(DateTimeOffset expiryDate, DateTimeOffset currentTime)
        => currentTime < expiryDate;
}
