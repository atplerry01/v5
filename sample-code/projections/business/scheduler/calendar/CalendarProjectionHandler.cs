using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Scheduler.Calendar;

public sealed class CalendarProjectionHandler
{
    public string ProjectionName => "whyce.business.scheduler.calendar";

    public string[] EventTypes =>
    [
        "whyce.business.scheduler.calendar.created",
        "whyce.business.scheduler.calendar.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICalendarViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CalendarReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
