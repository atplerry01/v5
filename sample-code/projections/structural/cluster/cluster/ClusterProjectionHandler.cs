using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Cluster.Cluster;

public sealed class ClusterProjectionHandler
{
    public string ProjectionName => "whyce.structural.clusters.cluster";

    public string[] EventTypes =>
    [
        "whyce.structural.clusters.cluster.created",
        "whyce.structural.clusters.cluster.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IClusterViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ClusterReadModel
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
