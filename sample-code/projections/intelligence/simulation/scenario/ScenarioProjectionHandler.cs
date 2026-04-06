using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Scenario;

public sealed class ScenarioProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.scenario";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.scenario.created",
        "whyce.intelligence.simulation.scenario.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IScenarioViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ScenarioReadModel
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
