using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// T3I Anomaly Detection Engine — hardened identity anomaly detection.
///
/// Detects:
///   - Unusual login frequency
///   - Device switching anomalies
///   - Graph anomalies
///   - Anti-gaming: sudden trust improvement, oscillation, burst behavior
///
/// Emits: anomaly.flagged, anomaly.behavior-gaming-detected
/// Stateless. No persistence. Chain-verified input only.
/// No domain imports — uses engine-local types only.
/// </summary>
public sealed class IdentityAnomalyDetectionEngine
{
    private readonly IClock _clock;

    public IdentityAnomalyDetectionEngine(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public AnomalyResult Detect(DetectAnomaliesCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // FIX 5: Chain-verified input only
        if (!command.ChainVerified)
            throw new InvalidOperationException("CHAIN_VERIFICATION_REQUIRED: Anomaly engine rejects unverified input.");

        var flags = new List<AnomalyFlagDto>();
        var now = _clock.UtcNowOffset;

        // 1. Unusual login frequency
        if (command.RecentLogins.Count > 0)
        {
            var cutoff = now.AddHours(-24);
            var loginsInWindow = command.RecentLogins.Count(l => l.Timestamp >= cutoff);
            if (loginsInWindow > 20)
            {
                flags.Add(new AnomalyFlagDto(
                    AnomalyFlagTypes.UnusualLoginFrequency,
                    $"{loginsInWindow} logins in 24h window",
                    Math.Min(loginsInWindow / 40m, 1m)));
            }
        }

        // 2. Device switching anomalies
        if (command.RecentDeviceEvents.Count > 0)
        {
            var distinctDevices = command.RecentDeviceEvents
                .Select(d => d.DeviceId).Distinct().Count();
            if (distinctDevices > 5)
            {
                flags.Add(new AnomalyFlagDto(
                    AnomalyFlagTypes.DeviceSwitchingAnomaly,
                    $"{distinctDevices} distinct devices in recent activity",
                    Math.Min(distinctDevices / 10m, 1m)));
            }
        }

        // 3. Failed login spike
        var failedLogins = command.RecentLogins.Count(l => !l.Success);
        if (failedLogins > 5)
        {
            flags.Add(new AnomalyFlagDto(
                AnomalyFlagTypes.AccessPatternAnomaly,
                $"{failedLogins} failed login attempts",
                Math.Min(failedLogins / 15m, 1m)));
        }

        // 4. Graph anomalies
        if (command.GraphNodeCount > 0 && command.GraphEdgeCount == 0)
        {
            flags.Add(new AnomalyFlagDto(
                AnomalyFlagTypes.GraphAnomaly,
                "Identity has nodes but no edges — isolated graph",
                0.4m));
        }

        // FIX 3: Anti-gaming detection
        if (command.TrustHistory is { Count: >= 3 })
        {
            var history = command.TrustHistory.OrderBy(h => h.ComputedAt).ToList();

            // 3a. Sudden improvement: >25 points in <48h
            for (var i = 1; i < history.Count; i++)
            {
                var improvement = history[i].Score - history[i - 1].Score;
                var hours = (history[i].ComputedAt - history[i - 1].ComputedAt).TotalHours;
                if (improvement > 25m && hours < 48)
                {
                    flags.Add(new AnomalyFlagDto(
                        AntiGamingTypes.SuddenImprovement,
                        $"Trust jumped {improvement:F1} points in {hours:F0}h",
                        0.85m));
                    break;
                }
            }

            // 3b. Oscillation detection: 3+ direction changes in recent history
            var directionChanges = 0;
            for (var i = 2; i < history.Count; i++)
            {
                var prev = history[i - 1].Score - history[i - 2].Score;
                var curr = history[i].Score - history[i - 1].Score;
                if ((prev > 0 && curr < 0) || (prev < 0 && curr > 0))
                    directionChanges++;
            }
            if (directionChanges >= 3)
            {
                flags.Add(new AnomalyFlagDto(
                    AntiGamingTypes.Oscillation,
                    $"Trust score oscillating: {directionChanges} direction changes",
                    0.7m));
            }

            // 3c. Burst behavior: rapid consecutive improvements
            var burstCount = 0;
            for (var i = 1; i < history.Count; i++)
            {
                var improvement = history[i].Score - history[i - 1].Score;
                if (improvement > 5m)
                    burstCount++;
                else
                    burstCount = 0;

                if (burstCount >= 3)
                {
                    flags.Add(new AnomalyFlagDto(
                        AntiGamingTypes.BurstBehavior,
                        $"Burst pattern: {burstCount} consecutive trust improvements",
                        0.75m));
                    break;
                }
            }
        }

        return new AnomalyResult(
            command.IdentityId,
            flags.Count > 0,
            flags,
            now);
    }

    /// <summary>
    /// Engine-local anomaly flag type constants — decoupled from domain AnomalyFlag.Types.
    /// </summary>
    public static class AnomalyFlagTypes
    {
        public const string UnusualLoginFrequency = "unusual_login_frequency";
        public const string DeviceSwitchingAnomaly = "device_switching_anomaly";
        public const string AccessPatternAnomaly = "access_pattern_anomaly";
        public const string GraphAnomaly = "graph_anomaly";
    }

    public static class AntiGamingTypes
    {
        public const string SuddenImprovement = "behavior_gaming_sudden_improvement";
        public const string Oscillation = "behavior_gaming_oscillation";
        public const string BurstBehavior = "behavior_gaming_burst";
    }
}
