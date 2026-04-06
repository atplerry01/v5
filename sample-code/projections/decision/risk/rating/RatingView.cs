namespace Whycespace.Projections.Decision.Risk.Rating;

public sealed record RatingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
