namespace Whycespace.Projections.Intelligence.Index.RegionalIndex;

public sealed record RegionalIndexView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
