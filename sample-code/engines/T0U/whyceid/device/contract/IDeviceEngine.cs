namespace Whycespace.Engines.T0U.WhyceId.Device;

public interface IDeviceEngine
{
    DeviceDecisionResult Evaluate(DeviceDecisionCommand command);
}

public sealed record DeviceDecisionCommand(
    string IdentityId,
    string DeviceFingerprint,
    string DeviceType,
    bool IsKnownDevice);

public sealed record DeviceDecisionResult(
    bool IsTrusted,
    string RiskLevel,
    string? Reason = null)
{
    public static DeviceDecisionResult Trusted() => new(true, "Low");
    public static DeviceDecisionResult Untrusted(string reason) => new(false, "High", reason);
    public static DeviceDecisionResult RequiresVerification() => new(false, "Medium", "Device requires verification.");
}
