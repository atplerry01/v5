using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Cluster.Classification;

public sealed class ClassificationProjectionHandler
{
    public string ProjectionName => "whyce.structural.clusters.classification";

    public string[] EventTypes =>
    [
        "whyce.structural.clusters.classification.created",
        "whyce.structural.clusters.classification.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IClassificationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ClassificationReadModel
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
