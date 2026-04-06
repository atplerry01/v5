using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Alert;

public sealed class AlertProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.alert";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.alert.created",
        "whyce.decision.risk.alert.updated"
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
