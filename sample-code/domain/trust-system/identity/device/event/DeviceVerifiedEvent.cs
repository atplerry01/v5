namespace Whycespace.Domain.TrustSystem.Identity.Device;

/// <summary>Topic: whyce.identity.device.verified</summary>
public sealed record DeviceVerifiedEvent(
    Guid DeviceId,
    Guid IdentityId) : DomainEvent;
