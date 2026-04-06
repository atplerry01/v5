namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// A node representing an organization in the intelligence graph.
/// </summary>
public sealed class OrganizationNode : Entity
{
    public required string OrganizationId { get; init; }
    public required string Name { get; init; }
    public int MemberCount { get; init; }
}
