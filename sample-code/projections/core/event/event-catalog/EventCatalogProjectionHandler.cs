using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Event.EventCatalog;

public sealed class EventCatalogProjectionHandler
{
    public string ProjectionName => "whyce.core.event.event-catalog";

    public string[] EventTypes =>
    [
        "whyce.core.event.event-catalog.created",
        "whyce.core.event.event-catalog.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEventCatalogViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EventCatalogReadModel
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
