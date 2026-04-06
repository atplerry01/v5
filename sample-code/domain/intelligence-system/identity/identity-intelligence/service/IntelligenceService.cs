namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Domain service for identity intelligence operations.
/// Coordinates scoring and anomaly assessment across aggregates.
/// Stateless — no persistence, no side effects beyond domain events.
/// </summary>
public static class IntelligenceService
{
    /// <summary>
    /// Determines if an identity should be flagged for review based on combined scores.
    /// </summary>
    public static bool ShouldFlagForReview(TrustScore trust, RiskScore risk)
    {
        return trust.Value < 30m || risk.Value > 60m;
    }

    /// <summary>
    /// Computes a combined intelligence score for policy context.
    /// Range: -100 (worst) to +100 (best).
    /// </summary>
    public static decimal ComputeIntelligenceScore(TrustScore trust, RiskScore risk)
    {
        return trust.Value - risk.Value;
    }
}
