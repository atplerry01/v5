using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.Revocation;

public sealed class RevocationProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.revocation";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.revocation.created",
        "whyce.business.entitlement.revocation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRevocationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RevocationReadModel
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
