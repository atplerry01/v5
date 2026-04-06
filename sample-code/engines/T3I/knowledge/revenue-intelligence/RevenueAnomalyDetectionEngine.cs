using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.RevenueIntelligence;

/// <summary>
/// T3I Revenue Anomaly Detection Engine -- detects anomalies in revenue flows.
///
/// Detects:
///   - Unusual revenue spikes/drops relative to baseline
///   - Payout frequency anomalies (unusual count in window)
///   - Charge pattern anomalies (excessive waivers)
///   - Anti-gaming: rapid charge reversals
///
/// Stateless. No persistence. Pure computation only.
/// No domain imports -- uses engine-local types only.
/// </summary>
public sealed class RevenueAnomalyDetectionEngine
{
    private readonly IClock _clock;

    public RevenueAnomalyDetectionEngine(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public RevenueAnomalyResult Detect(DetectRevenueAnomalyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var flags = new List<RevenueAnomalyFlagDto>();

        // 1. Revenue spike detection
        if (command.CurrentRevenue > 0 && command.BaselineRevenue > 0)
        {
            var ratio = command.CurrentRevenue / command.BaselineRevenue;
            if (ratio > 3.0m)
            {
                flags.Add(new RevenueAnomalyFlagDto(
                    AnomalyTypes.RevenueSurge,
                    "Revenue exceeds 3x baseline",
                    0.9m));
            }
            else if (ratio < 0.3m)
            {
                flags.Add(new RevenueAnomalyFlagDto(
                    AnomalyTypes.RevenueDrop,
                    "Revenue below 30% of baseline",
                    0.85m));
            }
        }

        // 2. Payout frequency anomaly
        if (command.PayoutCountInWindow > command.ExpectedPayoutCount * 2)
        {
            flags.Add(new RevenueAnomalyFlagDto(
                AnomalyTypes.PayoutFrequencySpike,
                "Payout frequency exceeds 2x expected",
                0.8m));
        }

        // 3. Charge pattern anomaly -- excessive waivers
        if (command.WaivedChargeCount > command.TotalChargeCount * 0.5m && command.TotalChargeCount > 10)
        {
            flags.Add(new RevenueAnomalyFlagDto(
                AnomalyTypes.ExcessiveWaivers,
                "Over 50% of charges waived",
                0.75m));
        }

        // 4. Anti-gaming: rapid charge reversals
        if (command.ReversalCountInWindow > 5)
        {
            flags.Add(new RevenueAnomalyFlagDto(
                AnomalyTypes.RapidReversals,
                "Excessive charge reversals detected",
                0.85m));
        }

        return new RevenueAnomalyResult(flags, flags.Count > 0);
    }

    /// <summary>
    /// Engine-local anomaly type constants for revenue anomaly detection.
    /// </summary>
    private static class AnomalyTypes
    {
        public const string RevenueSurge = "REVENUE_SURGE";
        public const string RevenueDrop = "REVENUE_DROP";
        public const string PayoutFrequencySpike = "PAYOUT_FREQUENCY_SPIKE";
        public const string ExcessiveWaivers = "EXCESSIVE_WAIVERS";
        public const string RapidReversals = "RAPID_REVERSALS";
    }
}

// -- Commands --

public sealed record DetectRevenueAnomalyCommand(
    decimal CurrentRevenue,
    decimal BaselineRevenue,
    int PayoutCountInWindow,
    int ExpectedPayoutCount,
    int WaivedChargeCount,
    int TotalChargeCount,
    int ReversalCountInWindow);

// -- Results --

public sealed record RevenueAnomalyResult(
    IReadOnlyList<RevenueAnomalyFlagDto> Flags,
    bool HasAnomalies);

public sealed record RevenueAnomalyFlagDto(
    string Type,
    string Description,
    decimal Confidence);
