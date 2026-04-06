using Whycespace.Engines.T0U.WhycePolicy.Evaluation;
using Whycespace.Engines.T0U.WhycePolicy.Registry;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhycePolicy.Federation;

/// <summary>
/// T0U federation handler — orchestrates cross-cluster policy evaluation.
/// Steps: build graph → validate cycles → validate compatibility →
///        resolve order → evaluate each → collect results.
/// NO runtime logic, NO infrastructure access.
/// Uses shared contracts instead of domain aggregate imports.
/// </summary>
public sealed class PolicyFederationHandler : IPolicyFederationHandler
{
    private readonly IPolicyEvaluationEngine _evaluationEngine;
    private readonly IPolicyFederationGraphService _graphService;
    private readonly IClock _clock;

    public PolicyFederationHandler(
        IPolicyEvaluationEngine evaluationEngine,
        IPolicyFederationGraphService graphService,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(evaluationEngine);
        _evaluationEngine = evaluationEngine;
        _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<FederationHandlerResult> HandleAsync(
        FederationHandlerInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // 1. Build graph from input using graph service contract
        var nodeInputs = input.Nodes
            .Select(n => new Whycespace.Shared.Contracts.Domain.Constitutional.Policy.FederationNodeInput(
                n.PolicyId.ToString(), n.Version.ToString(), n.ClusterId))
            .ToList();

        var edgeInputs = input.Edges
            .Select(e => new Whycespace.Shared.Contracts.Domain.Constitutional.Policy.FederationEdgeInput(
                e.SourcePolicyId.ToString(), e.TargetPolicyId.ToString(), e.RelationType))
            .ToList();

        var federationId = DeterministicIdHelper.FromSeed($"Federation:{input.ActorId}:{input.Action}:{input.Resource}:{input.Nodes.Count}");
        var graph = _graphService.BuildGraph(federationId, nodeInputs, edgeInputs);

        // 2. Check for circular dependencies
        if (_graphService.HasCircularDependencies(graph))
        {
            return new FederationHandlerResult(
                federationId,
                graph.GraphHash,
                "DENY",
                false,
                [],
                [],
                [],
                ["Circular dependency detected in federation graph — evaluation aborted."]);
        }

        // 3. Check cross-cluster compatibility
        var warnings = new List<string>();
        if (_graphService.HasCompatibilityIssues(graph))
        {
            warnings.Add("Cross-cluster compatibility issues detected.");
        }

        // 4. Resolve deterministic evaluation order via topological sort
        var evaluationOrder = _graphService.ResolveEvaluationOrder(graph);

        // 5. Evaluate each policy in dependency order
        var policyResults = new List<FederationPolicyResult>();
        var crossClusterConflicts = new List<string>();
        var dependencyChain = evaluationOrder
            .Select(n => $"{n.ClusterId}:{n.PolicyId}")
            .ToList();

        var finalDecision = "ALLOW";
        var isCompliant = true;
        var timestamp = input.Timestamp ?? _clock.UtcNowOffset;
        var environment = input.Environment ?? "default";

        foreach (var node in evaluationOrder)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodePolicyId = Guid.TryParse(node.PolicyId, out var parsedPolicyId) ? parsedPolicyId : Guid.Empty;

            var context = PolicyContext.Create(
                input.ActorId,
                input.Action,
                input.Resource,
                environment,
                policyId: nodePolicyId,
                timestamp: timestamp);

            var result = await _evaluationEngine.EvaluateAsync(context, cancellationToken);

            var policyResult = new FederationPolicyResult(
                nodePolicyId,
                node.ClusterId,
                result.DecisionType,
                result.IsAllowed);

            policyResults.Add(policyResult);

            if (result.IsDenied)
            {
                finalDecision = "DENY";
                isCompliant = false;
            }
        }

        // 6. Detect cross-cluster conflicts (different clusters, different decisions)
        var clusterGroups = policyResults
            .GroupBy(r => r.ClusterId)
            .ToList();

        if (clusterGroups.Count > 1)
        {
            var clusterDecisions = clusterGroups
                .Select(g => (
                    Cluster: g.Key,
                    HasDeny: g.Any(r => r.DecisionType == "DENY"),
                    HasAllow: g.Any(r => r.DecisionType == "ALLOW")))
                .ToList();

            var denyingClusters = clusterDecisions.Where(c => c.HasDeny).Select(c => c.Cluster).ToList();
            var allowingClusters = clusterDecisions.Where(c => c.HasAllow && !c.HasDeny).Select(c => c.Cluster).ToList();

            if (denyingClusters.Count > 0 && allowingClusters.Count > 0)
            {
                crossClusterConflicts.Add(
                    $"Decision conflict: clusters [{string.Join(", ", denyingClusters)}] DENY " +
                    $"while clusters [{string.Join(", ", allowingClusters)}] ALLOW.");
            }
        }

        return new FederationHandlerResult(
            graph.FederationId,
            graph.GraphHash,
            finalDecision,
            isCompliant,
            policyResults,
            dependencyChain,
            crossClusterConflicts,
            warnings);
    }

    public async Task<FederationHandlerResult> HandleIncrementalAsync(
        IncrementalFederationInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.PreviousGraph);

        // 1. Rebuild previous graph nodes + apply additions/removals
        var mergedNodes = input.PreviousGraph.Nodes.ToList();

        var removedNodeKeys = input.RemovedNodes
            .Select(n => (n.PolicyId, n.ClusterId))
            .ToHashSet();
        mergedNodes.RemoveAll(n => removedNodeKeys.Contains((n.PolicyId, n.ClusterId)));

        mergedNodes.AddRange(input.AddedNodes);

        // 2. Rebuild edges + apply additions/removals
        var mergedEdges = input.PreviousGraph.Edges.ToList();

        var removedEdgeKeys = input.RemovedEdges
            .Select(e => (e.SourcePolicyId, e.TargetPolicyId, e.RelationType))
            .ToHashSet();
        mergedEdges.RemoveAll(e => removedEdgeKeys.Contains((e.SourcePolicyId, e.TargetPolicyId, e.RelationType)));

        mergedEdges.AddRange(input.AddedEdges);

        // 3. Delegate to full HandleAsync — MUST produce same result as full rebuild
        var fullInput = new FederationHandlerInput(
            mergedNodes,
            mergedEdges,
            input.PreviousGraph.ActorId,
            input.PreviousGraph.Action,
            input.PreviousGraph.Resource,
            input.PreviousGraph.Environment,
            input.PreviousGraph.Timestamp);

        return await HandleAsync(fullInput, cancellationToken);
    }
}
