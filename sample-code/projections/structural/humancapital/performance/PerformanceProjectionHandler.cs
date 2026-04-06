using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Performance;

public sealed class PerformanceProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.performance";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.performance.created",
        "whyce.structural.humancapital.performance.updated"
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
