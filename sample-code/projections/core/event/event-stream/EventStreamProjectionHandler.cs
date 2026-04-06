using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Event.EventStream;

public sealed class EventStreamProjectionHandler
{
    public string ProjectionName => "whyce.core.event.event-stream";

    public string[] EventTypes =>
    [
        "whyce.core.event.event-stream.created",
        "whyce.core.event.event-stream.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEventStreamViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EventStreamReadModel
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
