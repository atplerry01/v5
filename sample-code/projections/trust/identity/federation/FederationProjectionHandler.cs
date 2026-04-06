using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Federation;

public sealed class FederationProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.federation";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.federation.created",
        "whyce.trust.identity.federation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IFederationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new FederationReadModel
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
