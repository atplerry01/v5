namespace Whycespace.Engines.T0U.WhyceId.Device;

public sealed class DeviceHandler : IDeviceEngine
{
    public DeviceDecisionResult Evaluate(DeviceDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.DeviceFingerprint))
            return DeviceDecisionResult.Untrusted("Device fingerprint is required.");

        if (!command.IsKnownDevice)
            return DeviceDecisionResult.RequiresVerification();

        return DeviceDecisionResult.Trusted();
    }
}
