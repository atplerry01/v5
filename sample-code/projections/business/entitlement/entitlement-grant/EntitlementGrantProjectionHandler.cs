using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.EntitlementGrant;

public sealed class EntitlementGrantProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.entitlement-grant";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.entitlement-grant.created",
        "whyce.business.entitlement.entitlement-grant.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEntitlementGrantViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EntitlementGrantReadModel
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
