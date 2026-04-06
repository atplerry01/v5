namespace Whycespace.Domain.TrustSystem.Identity.Identity;

/// <summary>
/// Topic: whyce.identity.identity.deactivated
/// </summary>
public sealed record IdentityDeactivatedEvent(
    Guid IdentityId,
    string Reason) : DomainEvent;
