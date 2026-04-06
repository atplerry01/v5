using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Cluster.Continuity;

public sealed class ContinuityProjectionHandler
{
    public string ProjectionName => "whyce.structural.clusters.continuity";

    public string[] EventTypes =>
    [
        "whyce.structural.clusters.continuity.created",
        "whyce.structural.clusters.continuity.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IContinuityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ContinuityReadModel
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
