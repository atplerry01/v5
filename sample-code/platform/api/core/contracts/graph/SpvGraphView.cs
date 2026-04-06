namespace Whycespace.Platform.Api.Core.Contracts.Graph;

/// <summary>
/// Read-only SPV graph projection view.
/// Contains nodes (SPVs), edges (relationships), and flows (capital movement).
/// Sourced from pre-built graph projections — no domain traversal,
/// no recursive queries, no N+1, no relationship computation.
/// </summary>
public sealed record SpvGraphView
{
    public required Guid RootSpvId { get; init; }
    public required IReadOnlyList<SpvNodeView> Nodes { get; init; }
    public required IReadOnlyList<SpvEdgeView> Edges { get; init; }
    public IReadOnlyList<SpvFlowView> Flows { get; init; } = [];
}
