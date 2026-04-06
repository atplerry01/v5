using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Portfolio.Benchmark;

public sealed class BenchmarkProjectionHandler
{
    public string ProjectionName => "whyce.business.portfolio.benchmark";

    public string[] EventTypes =>
    [
        "whyce.business.portfolio.benchmark.created",
        "whyce.business.portfolio.benchmark.updated"
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
