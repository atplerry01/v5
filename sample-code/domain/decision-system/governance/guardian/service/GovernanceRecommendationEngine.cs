namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Domain service that generates governance recommendations from analysis inputs.
/// Pure domain logic — no infrastructure dependencies.
/// </summary>
public sealed class GovernanceRecommendationEngine
{
    private readonly RecommendationValiditySpecification _validitySpec = new();

    public GovernanceRecommendationAggregate Generate(
        RecommendationSource source,
        IReadOnlyList<Guid> affectedPolicies,
        IReadOnlyList<string> suggestedChanges,
        double confidenceScore,
        double economicImpact,
        double operationalImpact,
        double governanceImpact,
        int riskScore,
        IReadOnlyList<RecommendationInsight> insights,
        DateTimeOffset timestamp)
    {
        var confidence = RecommendationConfidence.From(confidenceScore);
        var impact = ImpactEstimate.From(economicImpact, operationalImpact, governanceImpact);
        var risk = RiskEstimate.From(riskScore);

        var recommendation = GovernanceRecommendationAggregate.Create(
            source,
            affectedPolicies,
            suggestedChanges,
            confidence,
            impact,
            risk,
            insights,
            timestamp);

        if (!_validitySpec.IsSatisfiedBy(recommendation))
            throw new InvalidOperationException(InvalidRecommendationError.InvalidConfidence);

        return recommendation;
    }
}
