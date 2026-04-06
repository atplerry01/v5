namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class FederationDiffEngine
{
    /// <summary>
    /// Computes the diff between two federation graphs.
    /// Deterministic: same inputs always produce same diff.
    /// </summary>
    public FederationDiff ComputeDiff(PolicyFederationGraphAggregate previous, PolicyFederationGraphAggregate current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        // Nodes: match by PolicyId + ClusterId
        var prevNodeKeys = previous.Nodes.Select(n => (n.PolicyId, n.ClusterId)).ToHashSet();
        var currNodeKeys = current.Nodes.Select(n => (n.PolicyId, n.ClusterId)).ToHashSet();

        var addedNodes = current.Nodes
            .Where(n => !prevNodeKeys.Contains((n.PolicyId, n.ClusterId)))
            .ToList();
        var removedNodes = previous.Nodes
            .Where(n => !currNodeKeys.Contains((n.PolicyId, n.ClusterId)))
            .ToList();

        // Edges: match by SourcePolicyId + TargetPolicyId + RelationType
        var prevEdgeKeys = previous.Edges.Select(e => (e.SourcePolicyId, e.TargetPolicyId, e.RelationType.Value)).ToHashSet();
        var currEdgeKeys = current.Edges.Select(e => (e.SourcePolicyId, e.TargetPolicyId, e.RelationType.Value)).ToHashSet();

        var addedEdges = current.Edges
            .Where(e => !prevEdgeKeys.Contains((e.SourcePolicyId, e.TargetPolicyId, e.RelationType.Value)))
            .ToList();
        var removedEdges = previous.Edges
            .Where(e => !currEdgeKeys.Contains((e.SourcePolicyId, e.TargetPolicyId, e.RelationType.Value)))
            .ToList();

        return FederationDiff.Create(addedNodes, removedNodes, addedEdges, removedEdges);
    }
}
