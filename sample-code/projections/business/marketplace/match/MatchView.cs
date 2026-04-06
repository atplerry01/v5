namespace Whycespace.Projections.Business.Marketplace.Match;

public sealed record MatchView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
