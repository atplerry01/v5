namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// An edge connecting two nodes in the identity intelligence graph.
/// </summary>
public sealed class RelationshipEdge : Entity
{
    public required string SourceNodeId { get; init; }
    public required string TargetNodeId { get; init; }
    public required GraphEdgeType EdgeType { get; init; }
    public required RelationshipStrength Strength { get; init; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; init; }

    public void Deactivate() => IsActive = false;
}
