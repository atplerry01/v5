using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Temporal.Timeline;

public sealed class TimelineProjectionHandler
{
    public string ProjectionName => "whyce.core.temporal.timeline";

    public string[] EventTypes =>
    [
        "whyce.core.temporal.timeline.created",
        "whyce.core.temporal.timeline.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITimelineViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TimelineReadModel
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
