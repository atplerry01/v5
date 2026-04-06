using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Observability.Alert;

public sealed class AlertProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.observability.alert";

    public string[] EventTypes =>
    [
        "whyce.intelligence.observability.alert.created",
        "whyce.intelligence.observability.alert.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAlertViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AlertReadModel
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
