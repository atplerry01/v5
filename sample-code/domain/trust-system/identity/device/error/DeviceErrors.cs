namespace Whycespace.Domain.TrustSystem.Identity.Device;

public static class DeviceErrors
{
    public static DomainException NotFound(Guid deviceId)
        => new("DEVICE_NOT_FOUND", $"DeviceAggregate '{deviceId}' was not found.");

    public static DomainException AlreadyBlocked(Guid deviceId)
        => new("DEVICE_ALREADY_BLOCKED", $"DeviceAggregate '{deviceId}' is already blocked.");

    public static DomainException Deregistered(Guid deviceId)
        => new("DEVICE_DEREGISTERED", $"DeviceAggregate '{deviceId}' is deregistered.");
}
