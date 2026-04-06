using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentGrantedEvent(
    Guid ConsentId,
    Guid IdentityId,
    string ConsentType,
    string Scope,
    DateTimeOffset? ExpiryDate
) : DomainEvent;
