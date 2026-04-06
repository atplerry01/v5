using Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;
using Whycespace.Domain.ConstitutionalSystem.Policy.Registry;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Engine.Domain.Constitutional;

/// <summary>
/// Runtime implementation of IPolicyFederationGraphService — bridges to domain federation types.
/// </summary>
public sealed class PolicyFederationGraphService : IPolicyFederationGraphService
{
    private readonly IClock _clock;

    public PolicyFederationGraphService(IClock clock)
    {
        _clock = clock;
    }

    public FederationGraphResult BuildGraph(
        Guid federationId,
        IReadOnlyList<FederationNodeInput> nodes,
        IReadOnlyList<FederationEdgeInput> edges)
    {
        var builder = new FederationGraphBuilder();

        var nodeDefinitions = nodes
            .Select(n => new FederationNodeDefinition(Guid.Parse(n.PolicyId), int.Parse(n.Version), n.ClusterId))
            .ToList();

        var edgeDefinitions = edges
            .Select(e => new FederationEdgeDefinition(Guid.Parse(e.SourcePolicyId), Guid.Parse(e.TargetPolicyId), e.RelationType))
            .ToList();

        var graph = builder.Build(federationId, nodeDefinitions, edgeDefinitions, _clock.UtcNowOffset);

        return new FederationGraphResult(
            graph.FederationId,
            graph.GraphHash.Value,
            nodes,
            edges);
    }

    public bool HasCircularDependencies(FederationGraphResult graph)
    {
        var internalGraph = RebuildInternalGraph(graph);
        var spec = new NoCircularDependencySpecification();
        return !spec.IsSatisfiedBy(internalGraph);
    }

    public bool HasCompatibilityIssues(FederationGraphResult graph)
    {
        var internalGraph = RebuildInternalGraph(graph);
        var spec = new CrossClusterPolicyCompatibilitySpecification();
        return !spec.IsSatisfiedBy(internalGraph);
    }

    public IReadOnlyList<FederationNodeInput> ResolveEvaluationOrder(FederationGraphResult graph)
    {
        var internalGraph = RebuildInternalGraph(graph);
        var resolver = new FederationResolver();
        var order = resolver.ResolveEvaluationOrder(internalGraph);

        return order
            .Select(n => new FederationNodeInput(n.PolicyId.ToString(), n.Version.ToString(), n.ClusterId))
            .ToList();
    }

    private PolicyFederationGraphAggregate RebuildInternalGraph(FederationGraphResult graph)
    {
        var builder = new FederationGraphBuilder();

        var nodes = graph.Nodes
            .Select(n => new FederationNodeDefinition(Guid.Parse(n.PolicyId), int.Parse(n.Version), n.ClusterId))
            .ToList();

        var edges = graph.Edges
            .Select(e => new FederationEdgeDefinition(Guid.Parse(e.SourcePolicyId), Guid.Parse(e.TargetPolicyId), e.RelationType))
            .ToList();

        return builder.Build(graph.FederationId, nodes, edges, _clock.UtcNowOffset);
    }
}
