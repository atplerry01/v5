using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Device;

public readonly record struct DeviceDescriptor
{
    public Guid IdentityReference { get; }
    public string DeviceName { get; }
    public string DeviceType { get; }

    public DeviceDescriptor(Guid identityReference, string deviceName, string deviceType)
    {
        Guard.Against(identityReference == Guid.Empty, "DeviceDescriptor.IdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(deviceName), "DeviceDescriptor.DeviceName must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(deviceType), "DeviceDescriptor.DeviceType must not be empty.");

        IdentityReference = identityReference;
        DeviceName = deviceName.Trim();
        DeviceType = deviceType.Trim();
    }
}
