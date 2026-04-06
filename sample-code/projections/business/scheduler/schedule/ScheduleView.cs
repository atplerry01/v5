namespace Whycespace.Projections.Business.Scheduler.Schedule;

public sealed record ScheduleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
