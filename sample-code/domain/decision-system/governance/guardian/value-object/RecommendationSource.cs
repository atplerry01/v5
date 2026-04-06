namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Identifies the origin that triggered a governance recommendation.
/// </summary>
public enum RecommendationSource
{
    Simulation,
    Drift,
    Anomaly,
    Federation
}
