namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record RegisterDeviceResult(
    string DeviceId,
    string DeviceHash,
    bool IsRegistered);
