using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.EventBridge;

public sealed class EventBridgeProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.event-bridge";

    public string[] EventTypes =>
    [
        "whyce.business.integration.event-bridge.created",
        "whyce.business.integration.event-bridge.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEventBridgeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EventBridgeReadModel
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
