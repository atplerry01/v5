using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Economic.Optimization;

public sealed class OptimizationProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.economic.optimization";

    public string[] EventTypes =>
    [
        "whyce.intelligence.economic.optimization.created",
        "whyce.intelligence.economic.optimization.updated"
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
