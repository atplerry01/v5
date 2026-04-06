namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Validates that a recommendation's suggested changes comply with governance constraints.
/// </summary>
public sealed class GovernanceComplianceSpecification
{
    public bool IsSatisfiedBy(GovernanceRecommendationAggregate recommendation)
    {
        if (recommendation.Status == RecommendationStatus.Approved)
            return false;

        foreach (var change in recommendation.SuggestedChanges)
        {
            if (string.IsNullOrWhiteSpace(change))
                return false;
        }

        return recommendation.AffectedPolicies.Count > 0;
    }
}
