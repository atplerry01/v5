using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Estimation.Benchmark;

public sealed class BenchmarkProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.estimation.benchmark";

    public string[] EventTypes =>
    [
        "whyce.intelligence.estimation.benchmark.created",
        "whyce.intelligence.estimation.benchmark.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IBenchmarkViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new BenchmarkReadModel
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
