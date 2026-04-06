using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Observability.Metric;

public sealed class MetricProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.observability.metric";

    public string[] EventTypes =>
    [
        "whyce.intelligence.observability.metric.created",
        "whyce.intelligence.observability.metric.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMetricViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MetricReadModel
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
