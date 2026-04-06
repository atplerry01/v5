namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Governance guardrail specification that validates recommendations against safety constraints.
/// Flags unsafe recommendations as requiring manual review.
/// </summary>
public sealed class GovernanceGuardrailSpecification
{
    private readonly int _maxAllowedRisk;
    private readonly IReadOnlySet<string> _restrictedDomains;
    private readonly int _maxConflictThreshold;

    public GovernanceGuardrailSpecification(
        int maxAllowedRisk = 80,
        IReadOnlySet<string>? restrictedDomains = null,
        int maxConflictThreshold = 3)
    {
        _maxAllowedRisk = maxAllowedRisk;
        _restrictedDomains = restrictedDomains ?? new HashSet<string>();
        _maxConflictThreshold = maxConflictThreshold;
    }

    public GuardrailResult Evaluate(GovernanceRecommendationAggregate recommendation)
    {
        var violations = new List<string>();

        if (recommendation.Risk.Score > _maxAllowedRisk)
            violations.Add($"Risk score {recommendation.Risk.Score} exceeds maximum allowed {_maxAllowedRisk}.");

        if (recommendation.Confidence.IsLowConfidence)
            violations.Add($"Confidence {recommendation.Confidence.Value:F2} is below safe threshold.");

        var conflictInsights = recommendation.Insights.Count(i => i.Type == InsightType.Conflict);
        if (conflictInsights > _maxConflictThreshold)
            violations.Add($"Conflict count {conflictInsights} exceeds threshold {_maxConflictThreshold}.");

        return new GuardrailResult(
            RequiresManualReview: violations.Count > 0,
            Violations: violations);
    }
}

public sealed record GuardrailResult(
    bool RequiresManualReview,
    IReadOnlyList<string> Violations);
