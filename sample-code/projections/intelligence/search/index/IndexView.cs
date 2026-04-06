namespace Whycespace.Projections.Intelligence.Search.Index;

public sealed record IndexView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
