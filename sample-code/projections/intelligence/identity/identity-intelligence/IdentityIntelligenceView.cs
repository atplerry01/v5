namespace Whycespace.Projections.Intelligence.Identity.IdentityIntelligence;

public sealed record IdentityIntelligenceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
