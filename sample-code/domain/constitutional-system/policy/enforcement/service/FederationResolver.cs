namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class FederationResolver
{
    /// <summary>
    /// Resolves evaluation order via topological sort.
    /// Deterministic: sorts by cluster_id, policy_id, version within each level.
    /// </summary>
    public IReadOnlyList<FederationNode> ResolveEvaluationOrder(PolicyFederationGraphAggregate graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var dependsOnEdges = graph.Edges
            .Where(e => e.RelationType == FederationRelationType.DependsOn)
            .ToList();

        // Build adjacency: policy -> policies it depends on
        var inDegree = new Dictionary<Guid, int>();
        var dependents = new Dictionary<Guid, List<Guid>>();

        foreach (var node in graph.Nodes)
        {
            inDegree[node.PolicyId] = 0;
            dependents[node.PolicyId] = [];
        }

        foreach (var edge in dependsOnEdges)
        {
            // source DEPENDS_ON target -> target must evaluate first
            inDegree[edge.SourcePolicyId]++;
            dependents[edge.TargetPolicyId].Add(edge.SourcePolicyId);
        }

        // Kahn's algorithm with deterministic ordering
        var nodeMap = graph.Nodes.ToDictionary(n => n.PolicyId);

        var queue = new SortedSet<Guid>(
            inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key),
            Comparer<Guid>.Create((a, b) =>
            {
                var na = nodeMap[a];
                var nb = nodeMap[b];
                var cmp = string.Compare(na.ClusterId, nb.ClusterId, StringComparison.Ordinal);
                if (cmp != 0) return cmp;
                cmp = na.PolicyId.CompareTo(nb.PolicyId);
                if (cmp != 0) return cmp;
                return na.Version.CompareTo(nb.Version);
            }));

        var result = new List<FederationNode>();
        var visited = new HashSet<Guid>();

        while (queue.Count > 0)
        {
            var current = queue.Min;
            queue.Remove(current);

            if (!visited.Add(current)) continue;
            result.Add(nodeMap[current]);

            foreach (var dependent in dependents[current]
                         .OrderBy(d => nodeMap[d].ClusterId, StringComparer.Ordinal)
                         .ThenBy(d => d))
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                    queue.Add(dependent);
            }
        }

        if (result.Count != graph.Nodes.Count)
            throw new FederationCycleDetectedError(
                "Circular dependency detected in federation graph. " +
                $"Resolved {result.Count} of {graph.Nodes.Count} nodes.");

        return result.AsReadOnly();
    }
}
