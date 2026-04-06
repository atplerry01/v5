using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Control;

public sealed class ControlProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.control";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.control.created",
        "whyce.decision.risk.control.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IControlViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ControlReadModel
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
