namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class CrossClusterPolicyCompatibilitySpecification
{
    /// <summary>
    /// Validates that override and constraint relationships across clusters are compatible.
    /// A policy can only override/constrain a policy in a different cluster if both nodes are active.
    /// </summary>
    public bool IsSatisfiedBy(PolicyFederationGraphAggregate graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var nodeMap = graph.Nodes.ToDictionary(n => n.PolicyId);

        var crossClusterEdges = graph.Edges
            .Where(e => e.RelationType == FederationRelationType.Overrides ||
                        e.RelationType == FederationRelationType.Constrains)
            .Where(e => nodeMap.TryGetValue(e.SourcePolicyId, out var src) &&
                        nodeMap.TryGetValue(e.TargetPolicyId, out var tgt) &&
                        src.ClusterId != tgt.ClusterId)
            .ToList();

        foreach (var edge in crossClusterEdges)
        {
            var source = nodeMap[edge.SourcePolicyId];
            var target = nodeMap[edge.TargetPolicyId];

            if (source.Status != FederationNodeStatus.Active ||
                target.Status != FederationNodeStatus.Active)
                return false;
        }

        return true;
    }
}
