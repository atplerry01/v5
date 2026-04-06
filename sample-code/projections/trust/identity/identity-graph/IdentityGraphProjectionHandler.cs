using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.IdentityGraph;

public sealed class IdentityGraphProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.identity-graph";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.identity-graph.created",
        "whyce.trust.identity.identity-graph.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIdentityGraphViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IdentityGraphReadModel
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
