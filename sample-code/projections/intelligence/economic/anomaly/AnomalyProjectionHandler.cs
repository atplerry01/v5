using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Economic.Anomaly;

public sealed class AnomalyProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.economic.anomaly";

    public string[] EventTypes =>
    [
        "whyce.intelligence.economic.anomaly.created",
        "whyce.intelligence.economic.anomaly.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAnomalyViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AnomalyReadModel
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
