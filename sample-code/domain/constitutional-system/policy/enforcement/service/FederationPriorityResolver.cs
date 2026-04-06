namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

/// <summary>
/// Deterministic priority resolution for federation override conflicts.
/// Order: cluster_priority -> policy_priority -> version (latest wins).
/// No randomness. Stable across runs.
/// </summary>
public sealed class FederationPriorityResolver
{
    /// <summary>
    /// Resolves override priority among competing nodes.
    /// Nodes with overrides edges pointing to them are subordinate.
    /// When multiple overrides target the same policy, the highest priority source wins.
    /// </summary>
    public IReadOnlyList<FederationNode> ResolveOverridePriority(
        PolicyFederationGraphAggregate graph,
        IReadOnlyDictionary<string, int> clusterPriorities,
        IReadOnlyDictionary<Guid, int> policyPriorities)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(clusterPriorities);
        ArgumentNullException.ThrowIfNull(policyPriorities);

        return graph.Nodes
            .OrderByDescending(n => clusterPriorities.GetValueOrDefault(n.ClusterId, 0))
            .ThenByDescending(n => policyPriorities.GetValueOrDefault(n.PolicyId, 0))
            .ThenByDescending(n => n.Version)
            .ThenBy(n => n.ClusterId, StringComparer.Ordinal)
            .ThenBy(n => n.PolicyId)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Given competing override sources for a single target policy,
    /// returns the winning source (highest priority).
    /// Deterministic: same inputs -> same winner.
    /// </summary>
    public FederationNode? ResolveWinner(
        IReadOnlyList<FederationNode> candidates,
        IReadOnlyDictionary<string, int> clusterPriorities,
        IReadOnlyDictionary<Guid, int> policyPriorities)
    {
        if (candidates.Count == 0) return null;

        return candidates
            .OrderByDescending(n => clusterPriorities.GetValueOrDefault(n.ClusterId, 0))
            .ThenByDescending(n => policyPriorities.GetValueOrDefault(n.PolicyId, 0))
            .ThenByDescending(n => n.Version)
            .ThenBy(n => n.ClusterId, StringComparer.Ordinal)
            .ThenBy(n => n.PolicyId)
            .First();
    }
}
