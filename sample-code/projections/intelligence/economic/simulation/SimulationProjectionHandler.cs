using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Economic.Simulation;

public sealed class SimulationProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.economic.simulation";

    public string[] EventTypes =>
    [
        "whyce.intelligence.economic.simulation.created",
        "whyce.intelligence.economic.simulation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISimulationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SimulationReadModel
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
