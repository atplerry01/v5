using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Cost.CostBenchmark;

public sealed class CostBenchmarkProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.cost.cost-benchmark";

    public string[] EventTypes =>
    [
        "whyce.intelligence.cost.cost-benchmark.created",
        "whyce.intelligence.cost.cost-benchmark.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostBenchmarkViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostBenchmarkReadModel
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
