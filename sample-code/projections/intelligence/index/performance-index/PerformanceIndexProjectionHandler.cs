using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Index.PerformanceIndex;

public sealed class PerformanceIndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.index.performance-index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.index.performance-index.created",
        "whyce.intelligence.index.performance-index.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPerformanceIndexViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PerformanceIndexReadModel
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
