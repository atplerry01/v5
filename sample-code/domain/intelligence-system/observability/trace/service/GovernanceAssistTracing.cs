using System.Diagnostics;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

/// <summary>
/// Governance Assist (AGA) distributed tracing.
/// No business logic — observability only.
/// </summary>
public sealed class GovernanceAssistTracing
{
    private static readonly ActivitySource Source = new("Whycespace.GovernanceAssist");

    public static Activity? BeginAnalysis(Guid analysisId)
    {
        return Source.StartActivity(
            "governance.assist.analyze",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("aga.analysis_id", analysisId.ToString())
            ]);
    }

    public static void EndAnalysis(Activity? activity, int recommendationCount, int insightCount)
    {
        if (activity is null) return;

        activity.SetTag("aga.recommendations.count", recommendationCount);
        activity.SetTag("aga.insights.count", insightCount);
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginRecommendationGeneration(string source)
    {
        return Source.StartActivity(
            "governance.assist.recommend",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("aga.recommendation.source", source)
            ]);
    }

    public static void EndRecommendationGeneration(Activity? activity, int policyCount, double confidence)
    {
        if (activity is null) return;

        activity.SetTag("aga.recommendation.policies", policyCount);
        activity.SetTag("aga.recommendation.confidence", confidence);
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginProposalDraft(Guid recommendationId)
    {
        return Source.StartActivity(
            "governance.assist.draft",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("aga.recommendation_id", recommendationId.ToString())
            ]);
    }

    public static void EndProposalDraft(Activity? activity, int draftCount)
    {
        if (activity is null) return;

        activity.SetTag("aga.drafts.count", draftCount);
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginOptimization(int candidateCount, int maxRisk)
    {
        return Source.StartActivity(
            "governance.assist.optimize",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("aga.optimization.candidates", candidateCount),
                new("aga.optimization.max_risk", maxRisk)
            ]);
    }

    public static void EndOptimization(Activity? activity, bool success, int selectedCount)
    {
        if (activity is null) return;

        activity.SetTag("aga.optimization.success", success);
        activity.SetTag("aga.optimization.selected", selectedCount);
        activity.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        activity.Stop();
    }

    // --- E14.1 Hardening Tracing ---

    public static Activity? BeginTrustScoring(Guid recommendationId)
    {
        return Source.StartActivity(
            "governance.assist.trust",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("aga.recommendation_id", recommendationId.ToString())
            ]);
    }

    public static void EndTrustScoring(Activity? activity, double trustScore, bool isHighTrust)
    {
        if (activity is null) return;

        activity.SetTag("aga.trust.score", trustScore);
        activity.SetTag("aga.trust.high", isHighTrust);
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginBiasDetection(Guid recommendationId)
    {
        return Source.StartActivity(
            "governance.assist.bias",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("aga.recommendation_id", recommendationId.ToString())
            ]);
    }

    public static void EndBiasDetection(Activity? activity, bool biasDetected, string? biasType)
    {
        if (activity is null) return;

        activity.SetTag("aga.bias.detected", biasDetected);
        if (biasType is not null) activity.SetTag("aga.bias.type", biasType);
        activity.SetStatus(biasDetected ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginExplanationGeneration(Guid recommendationId)
    {
        return Source.StartActivity(
            "governance.assist.explain",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("aga.recommendation_id", recommendationId.ToString())
            ]);
    }

    public static void EndExplanationGeneration(Activity? activity, int nodeCount)
    {
        if (activity is null) return;

        activity.SetTag("aga.explanation.nodes", nodeCount);
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }
}
