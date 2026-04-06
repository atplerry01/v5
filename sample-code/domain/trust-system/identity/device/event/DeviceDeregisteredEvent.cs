namespace Whycespace.Domain.TrustSystem.Identity.Device;

/// <summary>Topic: whyce.identity.device.deregistered</summary>
public sealed record DeviceDeregisteredEvent(
    Guid DeviceId,
    Guid IdentityId,
    string Reason) : DomainEvent;
