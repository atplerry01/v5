using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Cluster.Lifecycle;

public sealed class LifecycleProjectionHandler
{
    public string ProjectionName => "whyce.structural.clusters.lifecycle";

    public string[] EventTypes =>
    [
        "whyce.structural.clusters.lifecycle.created",
        "whyce.structural.clusters.lifecycle.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILifecycleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LifecycleReadModel
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
