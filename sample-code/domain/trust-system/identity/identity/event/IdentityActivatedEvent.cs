namespace Whycespace.Domain.TrustSystem.Identity.Identity;

/// <summary>
/// Topic: whyce.identity.identity.activated
/// </summary>
public sealed record IdentityActivatedEvent(Guid IdentityId) : DomainEvent;
