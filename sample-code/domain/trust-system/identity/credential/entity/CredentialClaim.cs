using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CredentialClaim
{
    public Guid ClaimId { get; }
    public string ClaimType { get; }
    public string ClaimValue { get; }
    public DateTimeOffset IssuedAt { get; }

    public CredentialClaim(Guid claimId, string claimType, string claimValue, DateTimeOffset issuedAt)
    {
        ClaimId = claimId;
        ClaimType = claimType;
        ClaimValue = claimValue;
        IssuedAt = issuedAt;
    }
}
