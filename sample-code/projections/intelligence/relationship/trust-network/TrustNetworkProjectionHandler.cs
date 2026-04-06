using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Relationship.TrustNetwork;

public sealed class TrustNetworkProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.relationship.trust-network";

    public string[] EventTypes =>
    [
        "whyce.intelligence.relationship.trust-network.created",
        "whyce.intelligence.relationship.trust-network.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITrustNetworkViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TrustNetworkReadModel
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
