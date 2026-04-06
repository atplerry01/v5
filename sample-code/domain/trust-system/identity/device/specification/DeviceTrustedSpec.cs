namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed class DeviceTrustedSpec : Specification<DeviceAggregate>
{
    public override bool IsSatisfiedBy(DeviceAggregate entity)
        => entity.Status == DeviceStatus.Verified;
}
