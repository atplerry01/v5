namespace Whycespace.Projections.Intelligence.Search.Query;

public sealed record QueryView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
