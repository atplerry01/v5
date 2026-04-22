using Whycespace.Shared.Contracts.Events.Trust.Identity.Credential;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;

namespace Whycespace.Projections.Trust.Identity.Credential.Reducer;

public static class CredentialProjectionReducer
{
    public static CredentialReadModel Apply(CredentialReadModel state, CredentialIssuedEventSchema e)
        => state with
        {
            CredentialId = e.AggregateId,
            IdentityReference = e.IdentityReference,
            CredentialType = e.CredentialType,
            Status = "Issued",
            IssuedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static CredentialReadModel Apply(CredentialReadModel state, CredentialActivatedEventSchema _)
        => state with { Status = "Active", LastUpdatedAt = DateTimeOffset.UtcNow };

    public static CredentialReadModel Apply(CredentialReadModel state, CredentialRevokedEventSchema _)
        => state with { Status = "Revoked", LastUpdatedAt = DateTimeOffset.UtcNow };
}
