using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Event.EventDefinition;

public sealed class EventDefinitionProjectionHandler
{
    public string ProjectionName => "whyce.core.event.event-definition";

    public string[] EventTypes =>
    [
        "whyce.core.event.event-definition.created",
        "whyce.core.event.event-definition.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEventDefinitionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EventDefinitionReadModel
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
