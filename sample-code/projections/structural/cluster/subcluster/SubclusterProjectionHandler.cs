using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Cluster.Subcluster;

public sealed class SubclusterProjectionHandler
{
    public string ProjectionName => "whyce.structural.clusters.subcluster";

    public string[] EventTypes =>
    [
        "whyce.structural.clusters.subcluster.created",
        "whyce.structural.clusters.subcluster.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISubclusterViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SubclusterReadModel
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
