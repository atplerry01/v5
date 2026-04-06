namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// An anomaly flag detected for an identity.
/// Anomalies are advisory — enforcement is via policy.
/// </summary>
public sealed record AnomalyFlag(
    string AnomalyType,
    string Description,
    decimal Confidence,
    DateTimeOffset DetectedAt)
{
    public static class Types
    {
        public const string UnusualLoginFrequency = "unusual_login_frequency";
        public const string DeviceSwitchingAnomaly = "device_switching_anomaly";
        public const string GraphAnomaly = "graph_anomaly";
        public const string GeoVelocityAnomaly = "geo_velocity_anomaly";
        public const string SessionAnomaly = "session_anomaly";
        public const string AccessPatternAnomaly = "access_pattern_anomaly";
    }
}
