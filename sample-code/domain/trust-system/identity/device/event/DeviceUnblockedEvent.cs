namespace Whycespace.Domain.TrustSystem.Identity.Device;

/// <summary>Topic: whyce.identity.device.unblocked</summary>
public sealed record DeviceUnblockedEvent(
    Guid DeviceId,
    Guid IdentityId) : DomainEvent;
