using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentRevokedEvent(
    Guid ConsentId,
    Guid IdentityId,
    string Reason
) : DomainEvent;
