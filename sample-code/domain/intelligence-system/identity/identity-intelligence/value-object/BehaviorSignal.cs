namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// A discrete behavioral signal observed for an identity.
/// Signals are inputs to trust/risk scoring and anomaly detection.
/// </summary>
public sealed record BehaviorSignal(string SignalType, decimal Weight, DateTimeOffset ObservedAt)
{
    public static readonly BehaviorSignal NormalLogin = new("normal_login", 0m, default);
    public static readonly BehaviorSignal SuspiciousLogin = new("suspicious_login", 15m, default);
    public static readonly BehaviorSignal DeviceSwitch = new("device_switch", 5m, default);
    public static readonly BehaviorSignal RapidDeviceSwitch = new("rapid_device_switch", 25m, default);
    public static readonly BehaviorSignal FailedAuth = new("failed_auth", 10m, default);
    public static readonly BehaviorSignal PolicyViolation = new("policy_violation", 30m, default);
    public static readonly BehaviorSignal ConsentWithdrawn = new("consent_withdrawn", 20m, default);
    public static readonly BehaviorSignal VerificationCompleted = new("verification_completed", -15m, default);
    public static readonly BehaviorSignal LongSession = new("long_session", 5m, default);

    public BehaviorSignal At(DateTimeOffset timestamp) => this with { ObservedAt = timestamp };
}
