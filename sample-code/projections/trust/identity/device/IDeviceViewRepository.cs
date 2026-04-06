namespace Whycespace.Projections.Trust.Identity.Device;

public interface IDeviceViewRepository
{
    Task SaveAsync(DeviceReadModel model, CancellationToken ct = default);
    Task<DeviceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
