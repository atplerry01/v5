namespace Whycespace.Projections.Core.Temporal.TimeWindow;

public sealed record TimeWindowView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
