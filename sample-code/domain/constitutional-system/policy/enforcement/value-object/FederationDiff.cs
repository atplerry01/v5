namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class FederationDiff : ValueObject
{
    public IReadOnlyList<FederationNode> AddedNodes { get; }
    public IReadOnlyList<FederationNode> RemovedNodes { get; }
    public IReadOnlyList<FederationEdge> AddedEdges { get; }
    public IReadOnlyList<FederationEdge> RemovedEdges { get; }

    private FederationDiff(
        IReadOnlyList<FederationNode> addedNodes,
        IReadOnlyList<FederationNode> removedNodes,
        IReadOnlyList<FederationEdge> addedEdges,
        IReadOnlyList<FederationEdge> removedEdges)
    {
        AddedNodes = addedNodes;
        RemovedNodes = removedNodes;
        AddedEdges = addedEdges;
        RemovedEdges = removedEdges;
    }

    public static FederationDiff Create(
        IReadOnlyList<FederationNode> addedNodes,
        IReadOnlyList<FederationNode> removedNodes,
        IReadOnlyList<FederationEdge> addedEdges,
        IReadOnlyList<FederationEdge> removedEdges)
    {
        return new FederationDiff(
            addedNodes ?? [],
            removedNodes ?? [],
            addedEdges ?? [],
            removedEdges ?? []);
    }

    public bool IsEmpty => AddedNodes.Count == 0 && RemovedNodes.Count == 0 &&
                           AddedEdges.Count == 0 && RemovedEdges.Count == 0;

    public int TotalChanges => AddedNodes.Count + RemovedNodes.Count +
                               AddedEdges.Count + RemovedEdges.Count;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var n in AddedNodes.OrderBy(x => x.PolicyId)) yield return ($"+node:{n.PolicyId}");
        foreach (var n in RemovedNodes.OrderBy(x => x.PolicyId)) yield return ($"-node:{n.PolicyId}");
        foreach (var e in AddedEdges.OrderBy(x => x.SourcePolicyId).ThenBy(x => x.TargetPolicyId)) yield return ($"+edge:{e.SourcePolicyId}->{e.TargetPolicyId}");
        foreach (var e in RemovedEdges.OrderBy(x => x.SourcePolicyId).ThenBy(x => x.TargetPolicyId)) yield return ($"-edge:{e.SourcePolicyId}->{e.TargetPolicyId}");
    }
}
