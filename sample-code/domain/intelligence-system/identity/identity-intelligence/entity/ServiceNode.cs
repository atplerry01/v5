namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// A node representing a service identity in the intelligence graph.
/// </summary>
public sealed class ServiceNode : Entity
{
    public required string ServiceId { get; init; }
    public required string ServiceType { get; init; }
    public required string Status { get; init; }
}
