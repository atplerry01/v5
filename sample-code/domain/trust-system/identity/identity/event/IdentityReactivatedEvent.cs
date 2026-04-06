namespace Whycespace.Domain.TrustSystem.Identity.Identity;

/// <summary>
/// Topic: whyce.identity.identity.reactivated
/// </summary>
public sealed record IdentityReactivatedEvent(Guid IdentityId) : DomainEvent;
