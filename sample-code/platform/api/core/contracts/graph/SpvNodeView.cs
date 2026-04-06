namespace Whycespace.Platform.Api.Core.Contracts.Graph;

/// <summary>
/// Read-only SPV node in the graph projection.
/// Represents a single SPV entity with its structural position.
/// Sourced from pre-built graph projections — no domain traversal.
/// </summary>
public sealed record SpvNodeView
{
    public required Guid SpvId { get; init; }
    public required string Name { get; init; }
    public required string Cluster { get; init; }
    public required string SubCluster { get; init; }
    public required string Status { get; init; }
    public Guid? ParentSpvId { get; init; }
    public string? Jurisdiction { get; init; }
}
