namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// A node representing a session in the intelligence graph.
/// </summary>
public sealed class SessionNode : Entity
{
    public required string SessionId { get; init; }
    public required string IdentityId { get; init; }
    public required string DeviceId { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
