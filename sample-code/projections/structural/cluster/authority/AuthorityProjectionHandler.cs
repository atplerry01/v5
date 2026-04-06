using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Cluster.Authority;

public sealed class AuthorityProjectionHandler
{
    public string ProjectionName => "whyce.structural.clusters.authority";

    public string[] EventTypes =>
    [
        "whyce.structural.clusters.authority.created",
        "whyce.structural.clusters.authority.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAuthorityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AuthorityReadModel
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
