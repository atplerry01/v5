using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed record CredentialRevokedEvent(
    CredentialId CredentialId,
    Guid IdentityId
) : DomainEvent;
