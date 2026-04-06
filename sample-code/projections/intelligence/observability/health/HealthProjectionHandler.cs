using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Observability.Health;

public sealed class HealthProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.observability.health";

    public string[] EventTypes =>
    [
        "whyce.intelligence.observability.health.created",
        "whyce.intelligence.observability.health.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IHealthViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new HealthReadModel
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
