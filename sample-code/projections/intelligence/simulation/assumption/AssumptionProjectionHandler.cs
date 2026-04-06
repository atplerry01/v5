using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Assumption;

public sealed class AssumptionProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.assumption";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.assumption.created",
        "whyce.intelligence.simulation.assumption.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAssumptionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AssumptionReadModel
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
