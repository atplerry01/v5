using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.Right;

public sealed class RightProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.right";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.right.created",
        "whyce.business.entitlement.right.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRightViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RightReadModel
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
