namespace Whycespace.Projections.IdentityIntelligence.ReadModels;

public sealed record IdentityGraphReadModel
{
    public required string IdentityId { get; init; }
    public required IReadOnlyList<GraphNodeReadModel> Nodes { get; init; }
    public required IReadOnlyList<GraphEdgeReadModel> Edges { get; init; }
    public int NodeCount { get; init; }
    public int EdgeCount { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}

public sealed record GraphNodeReadModel
{
    public required string NodeId { get; init; }
    public required string NodeType { get; init; }
    public required string Status { get; init; }
}

public sealed record GraphEdgeReadModel
{
    public required string SourceNodeId { get; init; }
    public required string TargetNodeId { get; init; }
    public required string EdgeType { get; init; }
    public decimal Strength { get; init; }
}
