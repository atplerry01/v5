namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// A node in the identity intelligence graph representing an identity.
/// </summary>
public sealed class IdentityNode : Entity
{
    public required string IdentityId { get; init; }
    public required string IdentityType { get; init; }
    public required string Status { get; init; }
    public TrustScore TrustScore { get; init; } = TrustScore.Zero;
    public RiskScore RiskScore { get; init; } = RiskScore.None;
    public DateTimeOffset CreatedAt { get; init; }
}
