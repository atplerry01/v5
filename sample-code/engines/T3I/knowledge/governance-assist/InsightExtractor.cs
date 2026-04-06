using Whycespace.Engines.T3I.PolicySimulation;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Extracts governance insights from simulation results, detecting
/// recurring anomalies, high-risk patterns, and inefficiencies.
/// Read-only, deterministic, no side effects.
/// </summary>
public sealed class InsightExtractor
{
    public IReadOnlyList<RecommendationInsight> Extract(IReadOnlyList<PolicySimulationResult> simulationResults)
    {
        var insights = new List<RecommendationInsight>();

        foreach (var result in simulationResults)
        {
            ExtractAnomalyInsights(result, insights);
            ExtractDriftInsights(result, insights);
            ExtractInefficiencyInsights(result, insights);
            ExtractConflictInsights(result, insights);
        }

        return insights;
    }

    public IReadOnlyList<SystemInsight> ExtractSystemLevel(IReadOnlyList<PolicySimulationResult> simulationResults)
    {
        var systemInsights = new List<SystemInsight>();

        var anomalyCount = simulationResults.Sum(r => r.Anomalies.Count);
        if (anomalyCount > 0)
        {
            var affectedPolicies = simulationResults
                .Where(r => r.Anomalies.Count > 0)
                .SelectMany(r => r.PolicyVersionsUsed.Select(p => p.PolicyId))
                .Distinct()
                .ToList();

            systemInsights.Add(new SystemInsight
            {
                Category = "AnomalyCluster",
                Description = $"Detected {anomalyCount} anomalies across {affectedPolicies.Count} policies.",
                Severity = anomalyCount > 5 ? "High" : "Medium",
                RelatedPolicies = affectedPolicies
            });
        }

        var highRiskResults = simulationResults.Where(r => r.RiskScore is { Score: > 50 }).ToList();
        if (highRiskResults.Count > 0)
        {
            systemInsights.Add(new SystemInsight
            {
                Category = "RiskConcentration",
                Description = $"{highRiskResults.Count} simulation(s) exhibit elevated risk scores.",
                Severity = highRiskResults.Any(r => r.RiskScore!.Score > 75) ? "Critical" : "High",
                RelatedPolicies = highRiskResults
                    .SelectMany(r => r.PolicyVersionsUsed.Select(p => p.PolicyId))
                    .Distinct()
                    .ToList()
            });
        }

        return systemInsights;
    }

    private static void ExtractAnomalyInsights(PolicySimulationResult result, List<RecommendationInsight> insights)
    {
        foreach (var anomaly in result.Anomalies)
        {
            insights.Add(new RecommendationInsight(
                DeterministicIdHelper.FromSeed($"Insight:Anomaly:{result.ScenarioId}:{anomaly.Type}:{anomaly.Description}"),
                InsightType.Anomaly,
                $"[{anomaly.Type}] {anomaly.Description}",
                anomaly.Severity));
        }
    }

    private static void ExtractDriftInsights(PolicySimulationResult result, List<RecommendationInsight> insights)
    {
        if (result.Drift is not { DriftDetected: true }) return;

        insights.Add(new RecommendationInsight(
            DeterministicIdHelper.FromSeed($"Insight:Drift:{result.ScenarioId}:{result.Drift.DriftType}:{result.Drift.DriftMagnitude}"),
            InsightType.Drift,
            $"Policy drift detected: magnitude {result.Drift.DriftMagnitude:F2} ({result.Drift.DriftType}).",
            result.Drift.DriftMagnitude > 0.5 ? "High" : "Medium"));
    }

    private static void ExtractInefficiencyInsights(PolicySimulationResult result, List<RecommendationInsight> insights)
    {
        if (result.DecisionSummary.RulesFailed > result.DecisionSummary.RulesPassed)
        {
            insights.Add(new RecommendationInsight(
                DeterministicIdHelper.FromSeed($"Insight:Inefficiency:{result.ScenarioId}:{result.DecisionSummary.RulesFailed}:{result.DecisionSummary.RulesPassed}"),
                InsightType.Inefficiency,
                $"More rules failing ({result.DecisionSummary.RulesFailed}) than passing ({result.DecisionSummary.RulesPassed}) — policy may be overly restrictive.",
                "Medium"));
        }
    }

    private static void ExtractConflictInsights(PolicySimulationResult result, List<RecommendationInsight> insights)
    {
        if (result.CrossClusterConflicts is not { Count: > 0 }) return;

        foreach (var conflict in result.CrossClusterConflicts)
        {
            insights.Add(new RecommendationInsight(
                DeterministicIdHelper.FromSeed($"Insight:Conflict:{result.ScenarioId}:{conflict}"),
                InsightType.Conflict,
                $"Cross-cluster conflict: {conflict}",
                "High"));
        }
    }
}
