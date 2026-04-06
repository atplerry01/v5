using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed class DeviceAggregate : AggregateRoot
{
    public DeviceId DeviceId { get; private set; } = null!;
    public Guid IdentityId { get; private set; }
    public DeviceType DeviceType { get; private set; } = null!;
    public DeviceFingerprint Fingerprint { get; private set; } = null!;
    public DeviceStatus Status { get; private set; } = null!;
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }

    private DeviceAggregate() { }

    public static DeviceAggregate Register(
        Guid identityId,
        DeviceType deviceType,
        DeviceFingerprint fingerprint,
        DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(deviceType);
        Guard.AgainstNull(fingerprint);

        var device = new DeviceAggregate
        {
            DeviceId = DeviceId.FromSeed($"Device:{identityId}:{deviceType.Value}:{fingerprint.Value}"),
            IdentityId = identityId,
            DeviceType = deviceType,
            Fingerprint = fingerprint,
            Status = DeviceStatus.Registered,
            RegisteredAt = timestamp,
            LastSeenAt = timestamp
        };

        device.Id = device.DeviceId.Value;
        device.RaiseDomainEvent(new DeviceRegisteredEvent(
            device.DeviceId.Value, identityId, deviceType.Value, fingerprint.Value));
        return device;
    }

    public void Verify(DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Status == DeviceStatus.Registered,
            "DEVICE_MUST_BE_REGISTERED",
            "Only registered devices can be verified.");

        Status = DeviceStatus.Verified;
        VerifiedAt = timestamp;

        RaiseDomainEvent(new DeviceVerifiedEvent(DeviceId.Value, IdentityId));
    }

    public void Block(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureNotTerminal(Status, s => s == DeviceStatus.Deregistered, "Block");
        EnsureInvariant(
            Status != DeviceStatus.Blocked,
            "DEVICE_ALREADY_BLOCKED",
            "Device is already blocked.");

        Status = DeviceStatus.Blocked;
        RaiseDomainEvent(new DeviceBlockedEvent(DeviceId.Value, IdentityId, reason));
    }

    public void Unblock()
    {
        EnsureInvariant(
            Status == DeviceStatus.Blocked,
            "DEVICE_MUST_BE_BLOCKED",
            "Device is not blocked.");

        Status = DeviceStatus.Verified;
        RaiseDomainEvent(new DeviceUnblockedEvent(DeviceId.Value, IdentityId));
    }

    public void Deregister(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureNotTerminal(Status, s => s == DeviceStatus.Deregistered, "Deregister");

        Status = DeviceStatus.Deregistered;
        RaiseDomainEvent(new DeviceDeregisteredEvent(DeviceId.Value, IdentityId, reason));
    }

    public void RecordActivity(DateTimeOffset timestamp)
    {
        EnsureNotTerminal(Status, s => s == DeviceStatus.Deregistered, "RecordActivity");
        LastSeenAt = timestamp;
    }
}
