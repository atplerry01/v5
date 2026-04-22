namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record RegisterDeviceCommand(
    string IdentityId,
    string DeviceName,
    string DeviceType);
