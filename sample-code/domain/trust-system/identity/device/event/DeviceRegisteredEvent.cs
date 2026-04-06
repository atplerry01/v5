namespace Whycespace.Domain.TrustSystem.Identity.Device;

/// <summary>Topic: whyce.identity.device.registered</summary>
public sealed record DeviceRegisteredEvent(
    Guid DeviceId,
    Guid IdentityId,
    string DeviceType,
    string Fingerprint) : DomainEvent;
