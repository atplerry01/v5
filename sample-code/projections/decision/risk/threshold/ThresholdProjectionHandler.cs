using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Threshold;

public sealed class ThresholdProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.threshold";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.threshold.created",
        "whyce.decision.risk.threshold.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IThresholdViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ThresholdReadModel
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
