namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Validates that a recommendation meets minimum quality thresholds for governance review.
/// </summary>
public sealed class RecommendationValiditySpecification
{
    private const double MinConfidenceThreshold = 0.3;
    private const int MaxRiskThreshold = 90;

    public bool IsSatisfiedBy(GovernanceRecommendationAggregate recommendation)
    {
        if (recommendation.AffectedPolicies.Count == 0)
            return false;

        if (recommendation.Confidence.Value < MinConfidenceThreshold)
            return false;

        if (recommendation.Risk.Score > MaxRiskThreshold)
            return false;

        if (recommendation.Insights.Count == 0)
            return false;

        return true;
    }
}
