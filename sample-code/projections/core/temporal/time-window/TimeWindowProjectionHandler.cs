using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Temporal.TimeWindow;

public sealed class TimeWindowProjectionHandler
{
    public string ProjectionName => "whyce.core.temporal.time-window";

    public string[] EventTypes =>
    [
        "whyce.core.temporal.time-window.created",
        "whyce.core.temporal.time-window.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITimeWindowViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TimeWindowReadModel
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
