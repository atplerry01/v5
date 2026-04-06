using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Relationship.Influence;

public sealed class InfluenceProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.relationship.influence";

    public string[] EventTypes =>
    [
        "whyce.intelligence.relationship.influence.created",
        "whyce.intelligence.relationship.influence.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IInfluenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new InfluenceReadModel
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
