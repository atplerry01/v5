namespace Whycespace.Domain.TrustSystem.Identity.Identity;

/// <summary>
/// Topic: whyce.identity.identity.suspended
/// </summary>
public sealed record IdentitySuspendedEvent(
    Guid IdentityId,
    string Reason) : DomainEvent;
