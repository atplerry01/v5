namespace Whycespace.Domain.TrustSystem.Identity.Identity;

/// <summary>
/// Topic: whyce.identity.identity.registered
/// </summary>
public sealed record IdentityRegisteredEvent(
    Guid IdentityId,
    string IdentityType,
    string DisplayName) : DomainEvent;
