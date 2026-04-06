using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Optimization;

public sealed class OptimizationProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.optimization";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.optimization.created",
        "whyce.intelligence.simulation.optimization.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IOptimizationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new OptimizationReadModel
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
