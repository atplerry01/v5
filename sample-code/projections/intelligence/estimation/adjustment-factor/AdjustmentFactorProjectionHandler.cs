using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Estimation.AdjustmentFactor;

public sealed class AdjustmentFactorProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.estimation.adjustment-factor";

    public string[] EventTypes =>
    [
        "whyce.intelligence.estimation.adjustment-factor.created",
        "whyce.intelligence.estimation.adjustment-factor.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAdjustmentFactorViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AdjustmentFactorReadModel
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
