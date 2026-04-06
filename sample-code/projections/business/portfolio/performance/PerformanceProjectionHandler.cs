using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Portfolio.Performance;

public sealed class PerformanceProjectionHandler
{
    public string ProjectionName => "whyce.business.portfolio.performance";

    public string[] EventTypes =>
    [
        "whyce.business.portfolio.performance.created",
        "whyce.business.portfolio.performance.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPerformanceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PerformanceReadModel
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
