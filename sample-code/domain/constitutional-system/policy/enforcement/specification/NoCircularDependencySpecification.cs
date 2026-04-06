namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class NoCircularDependencySpecification
{
    public bool IsSatisfiedBy(PolicyFederationGraphAggregate graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var dependsOnEdges = graph.Edges
            .Where(e => e.RelationType == FederationRelationType.DependsOn)
            .ToList();

        // DFS cycle detection
        var adjacency = new Dictionary<Guid, List<Guid>>();
        foreach (var node in graph.Nodes)
            adjacency[node.PolicyId] = [];

        foreach (var edge in dependsOnEdges)
            adjacency[edge.SourcePolicyId].Add(edge.TargetPolicyId);

        var visiting = new HashSet<Guid>();
        var visited = new HashSet<Guid>();

        foreach (var node in graph.Nodes)
        {
            if (HasCycle(node.PolicyId, adjacency, visiting, visited))
                return false;
        }

        return true;
    }

    private static bool HasCycle(
        Guid nodeId,
        Dictionary<Guid, List<Guid>> adjacency,
        HashSet<Guid> visiting,
        HashSet<Guid> visited)
    {
        if (visiting.Contains(nodeId)) return true;
        if (visited.Contains(nodeId)) return false;

        visiting.Add(nodeId);

        if (adjacency.TryGetValue(nodeId, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (HasCycle(neighbor, adjacency, visiting, visited))
                    return true;
            }
        }

        visiting.Remove(nodeId);
        visited.Add(nodeId);
        return false;
    }
}
