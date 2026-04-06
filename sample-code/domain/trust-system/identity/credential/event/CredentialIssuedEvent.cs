using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed record CredentialIssuedEvent(
    CredentialId CredentialId,
    Guid IdentityId,
    CredentialType CredentialType,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiryDate
) : DomainEvent;
