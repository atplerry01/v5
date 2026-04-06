using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Movement;

public sealed class MovementProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.movement";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.movement.created",
        "whyce.business.inventory.movement.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMovementViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MovementReadModel
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
