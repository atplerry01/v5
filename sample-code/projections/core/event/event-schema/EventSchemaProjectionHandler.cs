using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Event.EventSchema;

public sealed class EventSchemaProjectionHandler
{
    public string ProjectionName => "whyce.core.event.event-schema";

    public string[] EventTypes =>
    [
        "whyce.core.event.event-schema.created",
        "whyce.core.event.event-schema.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEventSchemaViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EventSchemaReadModel
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
