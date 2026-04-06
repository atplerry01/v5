namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class FederationGraphBuilder
{
    public PolicyFederationGraphAggregate Build(
        Guid federationId,
        IReadOnlyList<FederationNodeDefinition> nodeDefinitions,
        IReadOnlyList<FederationEdgeDefinition> edgeDefinitions,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(nodeDefinitions);
        ArgumentNullException.ThrowIfNull(edgeDefinitions);

        var nodes = nodeDefinitions
            .Select(nd => FederationNode.Create(nd.PolicyId, nd.Version, nd.ClusterId))
            .ToList();

        var edges = edgeDefinitions
            .Select(ed => FederationEdge.Create(
                ed.SourcePolicyId,
                ed.TargetPolicyId,
                FederationRelationType.From(ed.RelationType)))
            .ToList();

        return PolicyFederationGraphAggregate.Create(federationId, nodes, edges, timestamp);
    }
}

public sealed record FederationNodeDefinition(Guid PolicyId, int Version, string ClusterId);
public sealed record FederationEdgeDefinition(Guid SourcePolicyId, Guid TargetPolicyId, string RelationType);
