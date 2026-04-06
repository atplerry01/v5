using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Relationship.Graph;

public sealed class GraphProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.relationship.graph";

    public string[] EventTypes =>
    [
        "whyce.intelligence.relationship.graph.created",
        "whyce.intelligence.relationship.graph.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGraphViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GraphReadModel
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
