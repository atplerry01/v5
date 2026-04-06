using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.Limit;

public sealed class LimitProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.limit";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.limit.created",
        "whyce.business.entitlement.limit.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILimitViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LimitReadModel
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
