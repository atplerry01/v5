using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentExpiredEvent(
    Guid ConsentId,
    Guid IdentityId
) : DomainEvent;
