namespace Whycespace.Projections.Intelligence.Search.Ranking;

public sealed record RankingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
