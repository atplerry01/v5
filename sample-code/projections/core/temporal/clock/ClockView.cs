namespace Whycespace.Projections.Core.Temporal.Clock;

public sealed record ClockView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
