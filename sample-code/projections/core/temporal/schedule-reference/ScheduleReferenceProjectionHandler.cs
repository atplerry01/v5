using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Temporal.ScheduleReference;

public sealed class ScheduleReferenceProjectionHandler
{
    public string ProjectionName => "whyce.core.temporal.schedule-reference";

    public string[] EventTypes =>
    [
        "whyce.core.temporal.schedule-reference.created",
        "whyce.core.temporal.schedule-reference.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IScheduleReferenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ScheduleReferenceReadModel
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
