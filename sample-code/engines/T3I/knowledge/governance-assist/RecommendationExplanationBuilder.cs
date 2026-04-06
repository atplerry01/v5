namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Builds hierarchical explanation trees for audit-ready recommendation explainability.
/// Structure: insight → simulation_result → impact → risk → recommendation.
/// Deterministic, read-only, no side effects.
/// </summary>
public sealed class RecommendationExplanationBuilder
{
    public ExplanationTree Build(
        GovernanceRecommendationAggregate recommendation,
        RecommendationTrustScore? trustScore,
        BiasAssessment? biasAssessment,
        GuardrailResult? guardrailResult)
    {
        var nodes = new List<ExplanationNode>();

        // Insight nodes
        var insightChildren = recommendation.Insights.Select(i => new ExplanationNode
        {
            Label = $"Insight: {i.Type}",
            Category = "Insight",
            Detail = i.Description,
            Children = []
        }).ToList();

        nodes.Add(new ExplanationNode
        {
            Label = "Analysis Inputs",
            Category = "Source",
            Detail = $"Source: {recommendation.Source}. {recommendation.Insights.Count} insight(s) identified.",
            Children = insightChildren
        });

        // Impact node
        nodes.Add(new ExplanationNode
        {
            Label = "Impact Assessment",
            Category = "Impact",
            Detail = $"Economic: {recommendation.Impact.EconomicScore:F2}, " +
                     $"Operational: {recommendation.Impact.OperationalScore:F2}, " +
                     $"Governance: {recommendation.Impact.GovernanceScore:F2}. " +
                     $"Composite: {recommendation.Impact.CompositeScore:F2}.",
            Children = []
        });

        // Risk node
        nodes.Add(new ExplanationNode
        {
            Label = "Risk Assessment",
            Category = "Risk",
            Detail = $"Score: {recommendation.Risk.Score}/100 ({recommendation.Risk.Category}). " +
                     (recommendation.Risk.IsCritical ? "CRITICAL: requires elevated review." : "Within acceptable bounds."),
            Children = []
        });

        // Trust node
        if (trustScore is not null)
        {
            nodes.Add(new ExplanationNode
            {
                Label = "Trust Assessment",
                Category = "Trust",
                Detail = $"Composite: {trustScore.CompositeScore:F2}. " +
                         $"Historical: {trustScore.HistoricalAccuracy:F2}, " +
                         $"Acceptance: {trustScore.AcceptanceRate:F2}, " +
                         $"Drift stability: {trustScore.DriftStability:F2}.",
                Children = []
            });
        }

        // Bias node
        if (biasAssessment is { BiasDetected: true })
        {
            nodes.Add(new ExplanationNode
            {
                Label = "Bias Warning",
                Category = "Bias",
                Detail = $"[{biasAssessment.Type}:{biasAssessment.Severity}] {biasAssessment.Description}",
                Children = []
            });
        }

        // Guardrail node
        if (guardrailResult is { RequiresManualReview: true })
        {
            var violationChildren = guardrailResult.Violations.Select(v => new ExplanationNode
            {
                Label = "Violation",
                Category = "Guardrail",
                Detail = v,
                Children = []
            }).ToList();

            nodes.Add(new ExplanationNode
            {
                Label = "Guardrail Violations",
                Category = "Guardrail",
                Detail = $"{guardrailResult.Violations.Count} guardrail violation(s) detected. Manual review required.",
                Children = violationChildren
            });
        }

        // Build reasoning summary
        var summary = BuildSummary(recommendation, trustScore, biasAssessment, guardrailResult);

        return new ExplanationTree
        {
            ReasoningSummary = summary,
            Nodes = nodes
        };
    }

    private static string BuildSummary(
        GovernanceRecommendationAggregate recommendation,
        RecommendationTrustScore? trustScore,
        BiasAssessment? biasAssessment,
        GuardrailResult? guardrailResult)
    {
        var parts = new List<string>
        {
            $"Recommendation from {recommendation.Source} analysis affecting {recommendation.AffectedPolicies.Count} policy(ies).",
            $"Confidence: {recommendation.Confidence.Value:F2}. Risk: {recommendation.Risk.Category} ({recommendation.Risk.Score}/100)."
        };

        if (trustScore is not null)
            parts.Add($"Trust: {trustScore.CompositeScore:F2} ({(trustScore.IsHighTrust ? "high" : trustScore.IsLowTrust ? "low" : "moderate")}).");

        if (biasAssessment is { BiasDetected: true })
            parts.Add($"Bias detected: {biasAssessment.Type} ({biasAssessment.Severity}).");

        if (guardrailResult is { RequiresManualReview: true })
            parts.Add($"Manual review required: {guardrailResult.Violations.Count} guardrail violation(s).");

        return string.Join(" ", parts);
    }
}
