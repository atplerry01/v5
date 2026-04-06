namespace Whycespace.Projections.Core.Temporal.ScheduleReference;

public sealed record ScheduleReferenceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
