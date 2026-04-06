namespace Whycespace.Platform.Api.Core.Contracts.Graph;

/// <summary>
/// Read-only SPV relationship edge in the graph projection.
/// Represents a directional relationship between two SPVs.
/// Sourced from pre-built graph projections — no relationship computation.
/// </summary>
public sealed record SpvEdgeView
{
    public required Guid FromSpvId { get; init; }
    public required Guid ToSpvId { get; init; }
    public required string RelationshipType { get; init; }
    public string? Label { get; init; }
}
