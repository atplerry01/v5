using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.Restriction;

public sealed class RestrictionProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.restriction";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.restriction.created",
        "whyce.business.entitlement.restriction.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRestrictionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RestrictionReadModel
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
