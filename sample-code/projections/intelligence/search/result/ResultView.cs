namespace Whycespace.Projections.Intelligence.Search.Result;

public sealed record ResultView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
