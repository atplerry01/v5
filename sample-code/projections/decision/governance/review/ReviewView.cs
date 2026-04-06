namespace Whycespace.Projections.Decision.Governance.Review;

public sealed record ReviewView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
