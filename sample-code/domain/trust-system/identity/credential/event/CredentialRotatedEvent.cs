using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed record CredentialRotatedEvent(
    CredentialId OldCredentialId,
    CredentialId NewCredentialId,
    Guid IdentityId,
    CredentialType CredentialType,
    DateTimeOffset NewExpiryDate
) : DomainEvent;
