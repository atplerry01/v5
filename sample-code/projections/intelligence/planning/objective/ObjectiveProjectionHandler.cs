using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Planning.Objective;

public sealed class ObjectiveProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.planning.objective";

    public string[] EventTypes =>
    [
        "whyce.intelligence.planning.objective.created",
        "whyce.intelligence.planning.objective.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IObjectiveViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ObjectiveReadModel
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
