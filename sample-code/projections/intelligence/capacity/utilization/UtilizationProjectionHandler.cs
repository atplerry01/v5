using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Capacity.Utilization;

public sealed class UtilizationProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.capacity.utilization";

    public string[] EventTypes =>
    [
        "whyce.intelligence.capacity.utilization.created",
        "whyce.intelligence.capacity.utilization.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IUtilizationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new UtilizationReadModel
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
