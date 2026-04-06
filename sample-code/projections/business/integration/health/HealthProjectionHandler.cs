using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Health;

public sealed class HealthProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.health";

    public string[] EventTypes =>
    [
        "whyce.business.integration.health.created",
        "whyce.business.integration.health.updated"
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
