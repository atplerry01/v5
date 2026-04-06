using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Scheduler.Schedule;

public sealed class ScheduleProjectionHandler
{
    public string ProjectionName => "whyce.business.scheduler.schedule";

    public string[] EventTypes =>
    [
        "whyce.business.scheduler.schedule.created",
        "whyce.business.scheduler.schedule.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IScheduleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ScheduleReadModel
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
