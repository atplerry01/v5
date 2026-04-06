namespace Whycespace.Projections.Business.Scheduler.Calendar;

public sealed record CalendarView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
