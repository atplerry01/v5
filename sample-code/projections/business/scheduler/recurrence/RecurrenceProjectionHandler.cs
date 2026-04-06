using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Scheduler.Recurrence;

public sealed class RecurrenceProjectionHandler
{
    public string ProjectionName => "whyce.business.scheduler.recurrence";

    public string[] EventTypes =>
    [
        "whyce.business.scheduler.recurrence.created",
        "whyce.business.scheduler.recurrence.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRecurrenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RecurrenceReadModel
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
