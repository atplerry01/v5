using System.Diagnostics;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

/// <summary>
/// Federation-specific distributed tracing.
/// No business logic — observability only.
/// </summary>
public sealed class FederationTracing
{
    private static readonly ActivitySource Source = new("Whycespace.Policy.Federation");

    public static Activity? BeginGraphBuild(int nodeCount, int edgeCount)
    {
        return Source.StartActivity(
            "federation.graph.build",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("federation.node_count", nodeCount),
                new("federation.edge_count", edgeCount)
            ]);
    }

    public static void EndGraphBuild(Activity? activity, string graphHash, bool hasCycles)
    {
        if (activity is null) return;
        activity.SetTag("federation.graph_hash", graphHash);
        activity.SetTag("federation.has_cycles", hasCycles);
        activity.SetStatus(hasCycles ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginDiffComputation(string previousHash, string currentHash)
    {
        return Source.StartActivity(
            "federation.diff.compute",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("federation.previous_hash", previousHash),
                new("federation.current_hash", currentHash)
            ]);
    }

    public static void EndDiffComputation(Activity? activity, int totalChanges)
    {
        if (activity is null) return;
        activity.SetTag("federation.diff.total_changes", totalChanges);
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginResolution(int nodeCount)
    {
        return Source.StartActivity(
            "federation.resolve",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("federation.node_count", nodeCount)
            ]);
    }

    public static void EndResolution(Activity? activity, int resolvedCount, bool success)
    {
        if (activity is null) return;
        activity.SetTag("federation.resolved_count", resolvedCount);
        activity.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        activity.Stop();
    }

    public static Activity? BeginEvaluation(string graphHash, int policyCount)
    {
        return Source.StartActivity(
            "federation.evaluate",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("federation.graph_hash", graphHash),
                new("federation.policy_count", policyCount)
            ]);
    }

    public static void EndEvaluation(Activity? activity, string decisionType, int conflictCount)
    {
        if (activity is null) return;
        activity.SetTag("federation.decision", decisionType);
        activity.SetTag("federation.conflict_count", conflictCount);
        activity.SetStatus(decisionType == "DENY" ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
        activity.Stop();
    }
}
