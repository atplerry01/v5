using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Cluster.Topology;

public sealed class TopologyProjectionHandler
{
    public string ProjectionName => "whyce.structural.clusters.topology";

    public string[] EventTypes =>
    [
        "whyce.structural.clusters.topology.created",
        "whyce.structural.clusters.topology.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITopologyViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TopologyReadModel
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
