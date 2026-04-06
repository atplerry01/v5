namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Type of edge in the identity intelligence graph.
/// Represents the relationship between two nodes.
/// </summary>
public sealed record GraphEdgeType(string Value)
{
    public static readonly GraphEdgeType OwnsDevice = new("owns_device");
    public static readonly GraphEdgeType HasSession = new("has_session");
    public static readonly GraphEdgeType BelongsToOrg = new("belongs_to_org");
    public static readonly GraphEdgeType UsesService = new("uses_service");
    public static readonly GraphEdgeType LinkedIdentity = new("linked_identity");
    public static readonly GraphEdgeType SharedDevice = new("shared_device");
    public static readonly GraphEdgeType DelegatedAccess = new("delegated_access");

    public override string ToString() => Value;
}
