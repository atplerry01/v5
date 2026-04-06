using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T3I.PolicySimulation.Federation;

/// <summary>
/// Federation simulation extension for the T3I Policy Simulation Engine.
/// Simulates multi-cluster policy evaluation and detects cross-cluster conflicts.
/// Read-only guarantee -- NO writes to Postgres, WhyceChain, or active policy state.
/// Composable: designed to be used alongside PolicySimulationEngine, not as a replacement.
/// </summary>
public sealed class FederationSimulationSupport
{
    private readonly IPolicyFederationEngine _federationEngine;

    public FederationSimulationSupport(IPolicyFederationEngine federationEngine)
    {
        ArgumentNullException.ThrowIfNull(federationEngine);
        _federationEngine = federationEngine;
    }

    /// <summary>
    /// Simulates federation-level policy evaluation across multiple clusters.
    /// Builds the federation graph, checks for cycles, evaluates policies,
    /// and returns a read-only simulation result with conflict detection.
    /// </summary>
    public async Task<FederationSimulationResult> SimulateFederationAsync(
        FederationSimulationInput input,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // 1. Build federation graph via engine
        var buildResult = await _federationEngine.BuildGraphAsync(
            new FederationBuildInput(input.Nodes, input.Edges), ct);

        if (buildResult.HasCycles)
        {
            return FederationSimulationResult.Failed(
                buildResult.GraphHash,
                "Federation graph contains circular dependencies.");
        }

        if (!buildResult.IsCompatible)
        {
            return FederationSimulationResult.Failed(
                buildResult.GraphHash,
                "Federation graph contains incompatible policy versions.");
        }

        // 2. Evaluate federation
        var evalResult = await _federationEngine.EvaluateAsync(
            new FederationEvaluationInput(
                buildResult.GraphHash,
                input.ActorId,
                input.Action,
                input.Resource,
                input.Environment,
                input.SimulatedTime), ct);

        // 3. Build simulation result
        return new FederationSimulationResult(
            GraphHash: buildResult.GraphHash,
            FederationId: buildResult.FederationId,
            NodeCount: buildResult.NodeCount,
            EdgeCount: buildResult.EdgeCount,
            DecisionType: evalResult.DecisionType,
            IsCompliant: evalResult.IsCompliant,
            PolicyDecisions: evalResult.PolicyDecisions,
            DependencyChain: evalResult.DependencyChain,
            CrossClusterConflicts: evalResult.CrossClusterConflicts,
            Warnings: buildResult.Warnings,
            Recommendation: DeriveRecommendation(evalResult));
    }

    /// <summary>
    /// Simulates multi-version federation — compares two graph versions
    /// and computes impact delta between them.
    /// </summary>
    public async Task<FederationVersionComparisonResult> CompareVersionsAsync(
        FederationSimulationInput beforeInput,
        FederationSimulationInput afterInput,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(beforeInput);
        ArgumentNullException.ThrowIfNull(afterInput);

        var beforeResult = await SimulateFederationAsync(beforeInput, ct);
        var afterResult = await SimulateFederationAsync(afterInput, ct);

        // Compute diff summary
        var addedPolicies = afterResult.PolicyDecisions
            .Where(a => !beforeResult.PolicyDecisions.Any(b => b.PolicyId == a.PolicyId))
            .Select(p => p.PolicyId)
            .ToList();

        var removedPolicies = beforeResult.PolicyDecisions
            .Where(b => !afterResult.PolicyDecisions.Any(a => a.PolicyId == b.PolicyId))
            .Select(p => p.PolicyId)
            .ToList();

        var changedDecisions = afterResult.PolicyDecisions
            .Where(a => beforeResult.PolicyDecisions.Any(b => b.PolicyId == a.PolicyId && b.DecisionType != a.DecisionType))
            .Select(a => new DecisionChange(
                a.PolicyId,
                beforeResult.PolicyDecisions.First(b => b.PolicyId == a.PolicyId).DecisionType,
                a.DecisionType))
            .ToList();

        var newConflicts = afterResult.CrossClusterConflicts
            .Where(c => !beforeResult.CrossClusterConflicts.Contains(c))
            .ToList();

        var resolvedConflicts = beforeResult.CrossClusterConflicts
            .Where(c => !afterResult.CrossClusterConflicts.Contains(c))
            .ToList();

        return new FederationVersionComparisonResult(
            BeforeGraphHash: beforeResult.GraphHash,
            AfterGraphHash: afterResult.GraphHash,
            BeforeDecision: beforeResult.DecisionType,
            AfterDecision: afterResult.DecisionType,
            AddedPolicies: addedPolicies,
            RemovedPolicies: removedPolicies,
            ChangedDecisions: changedDecisions,
            NewConflicts: newConflicts,
            ResolvedConflicts: resolvedConflicts,
            GraphSizeMetrics: new GraphSizeMetrics(
                beforeResult.NodeCount, beforeResult.EdgeCount,
                afterResult.NodeCount, afterResult.EdgeCount),
            ImpactAssessment: DeriveImpactAssessment(changedDecisions, newConflicts));
    }

    private static string DeriveRecommendation(FederationEvaluationResult result)
    {
        if (result.CrossClusterConflicts.Count > 0)
            return "BLOCK -- Cross-cluster conflicts detected. Resolve before activation.";

        if (!result.IsCompliant)
            return "REVIEW -- Federation evaluation is non-compliant. Governance review required.";

        return "PROCEED -- Federation simulation passed with no cross-cluster conflicts.";
    }

    private static string DeriveImpactAssessment(
        IReadOnlyList<DecisionChange> changes,
        IReadOnlyList<string> newConflicts)
    {
        if (newConflicts.Count > 0)
            return "HIGH -- New cross-cluster conflicts introduced.";
        if (changes.Any(c => c.Before == "ALLOW" && c.After == "DENY"))
            return "HIGH -- Policies changed from ALLOW to DENY.";
        if (changes.Count > 0)
            return "MEDIUM -- Decision changes detected.";
        return "LOW -- No significant impact.";
    }
}

// --- Federation Simulation Input/Output Types ---

public sealed record FederationSimulationInput(
    IReadOnlyList<FederationNodeDto> Nodes,
    IReadOnlyList<FederationEdgeDto> Edges,
    string ActorId,
    string Action,
    string Resource,
    string? Environment,
    DateTime? SimulatedTime);

public sealed record FederationSimulationResult(
    string GraphHash,
    Guid FederationId,
    int NodeCount,
    int EdgeCount,
    string DecisionType,
    bool IsCompliant,
    IReadOnlyList<FederationPolicyDecision> PolicyDecisions,
    IReadOnlyList<string> DependencyChain,
    IReadOnlyList<string> CrossClusterConflicts,
    IReadOnlyList<string> Warnings,
    string Recommendation)
{
    public long? FederationVersion { get; init; }
    public string? FederationDiffSummary { get; init; }
    public GraphSizeMetrics? SizeMetrics { get; init; }

    public static FederationSimulationResult Failed(string graphHash, string reason) =>
        new(graphHash, Guid.Empty, 0, 0, "DENY", false, [], [], [], [], reason);
}

public sealed record FederationVersionComparisonResult(
    string BeforeGraphHash,
    string AfterGraphHash,
    string BeforeDecision,
    string AfterDecision,
    IReadOnlyList<Guid> AddedPolicies,
    IReadOnlyList<Guid> RemovedPolicies,
    IReadOnlyList<DecisionChange> ChangedDecisions,
    IReadOnlyList<string> NewConflicts,
    IReadOnlyList<string> ResolvedConflicts,
    GraphSizeMetrics GraphSizeMetrics,
    string ImpactAssessment);

public sealed record DecisionChange(Guid PolicyId, string Before, string After);

public sealed record GraphSizeMetrics(
    int BeforeNodeCount, int BeforeEdgeCount,
    int AfterNodeCount, int AfterEdgeCount)
{
    public int NodeDelta => AfterNodeCount - BeforeNodeCount;
    public int EdgeDelta => AfterEdgeCount - BeforeEdgeCount;
}
