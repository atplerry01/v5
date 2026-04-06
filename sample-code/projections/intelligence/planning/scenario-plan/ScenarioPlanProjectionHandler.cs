using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Planning.ScenarioPlan;

public sealed class ScenarioPlanProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.planning.scenario-plan";

    public string[] EventTypes =>
    [
        "whyce.intelligence.planning.scenario-plan.created",
        "whyce.intelligence.planning.scenario-plan.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IScenarioPlanViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ScenarioPlanReadModel
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
