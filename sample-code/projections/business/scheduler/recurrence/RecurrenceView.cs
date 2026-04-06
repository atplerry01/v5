namespace Whycespace.Projections.Business.Scheduler.Recurrence;

public sealed record RecurrenceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
