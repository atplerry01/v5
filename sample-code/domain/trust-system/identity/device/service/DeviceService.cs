namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed class DeviceService
{
    public bool IsTrusted(DeviceAggregate device)
        => device.Status == DeviceStatus.Verified;
}
