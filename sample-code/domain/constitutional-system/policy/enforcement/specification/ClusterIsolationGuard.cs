namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

/// <summary>
/// Guards against unintended cross-cluster influence.
/// Requires explicit permission for cross-cluster overrides/constraints.
/// </summary>
public sealed class ClusterIsolationGuard
{
    /// <summary>
    /// Validates that all cross-cluster override/constraint edges have explicit permission.
    /// Permission is granted by listing allowed cross-cluster pairs.
    /// </summary>
    public ClusterIsolationResult Validate(
        PolicyFederationGraphAggregate graph,
        IReadOnlySet<(string SourceCluster, string TargetCluster)> allowedCrossClusterPairs)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(allowedCrossClusterPairs);

        var nodeMap = graph.Nodes.ToDictionary(n => n.PolicyId);
        var violations = new List<ClusterIsolationViolation>();

        var crossClusterEdges = graph.Edges
            .Where(e => e.RelationType == FederationRelationType.Overrides ||
                        e.RelationType == FederationRelationType.Constrains)
            .Where(e => nodeMap.ContainsKey(e.SourcePolicyId) &&
                        nodeMap.ContainsKey(e.TargetPolicyId))
            .Where(e => nodeMap[e.SourcePolicyId].ClusterId != nodeMap[e.TargetPolicyId].ClusterId)
            .ToList();

        foreach (var edge in crossClusterEdges)
        {
            var sourceCluster = nodeMap[edge.SourcePolicyId].ClusterId;
            var targetCluster = nodeMap[edge.TargetPolicyId].ClusterId;

            if (!allowedCrossClusterPairs.Contains((sourceCluster, targetCluster)))
            {
                violations.Add(new ClusterIsolationViolation(
                    edge.SourcePolicyId,
                    edge.TargetPolicyId,
                    sourceCluster,
                    targetCluster,
                    edge.RelationType.Value));
            }
        }

        return new ClusterIsolationResult(violations.Count == 0, violations.AsReadOnly());
    }
}

public sealed record ClusterIsolationViolation(
    Guid SourcePolicyId,
    Guid TargetPolicyId,
    string SourceCluster,
    string TargetCluster,
    string RelationType);

public sealed record ClusterIsolationResult(
    bool IsIsolated,
    IReadOnlyList<ClusterIsolationViolation> Violations);
