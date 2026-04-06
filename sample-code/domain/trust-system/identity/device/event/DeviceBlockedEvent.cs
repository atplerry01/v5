namespace Whycespace.Domain.TrustSystem.Identity.Device;

/// <summary>Topic: whyce.identity.device.blocked</summary>
public sealed record DeviceBlockedEvent(
    Guid DeviceId,
    Guid IdentityId,
    string Reason) : DomainEvent;
