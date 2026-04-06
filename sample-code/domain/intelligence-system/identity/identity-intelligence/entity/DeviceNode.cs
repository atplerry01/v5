namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// A node representing a device in the intelligence graph.
/// </summary>
public sealed class DeviceNode : Entity
{
    public required string DeviceId { get; init; }
    public required string DeviceType { get; init; }
    public required string Fingerprint { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastActivityAt { get; init; }
}
