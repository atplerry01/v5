using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Billing.Adjustment;

public sealed class AdjustmentProjectionHandler
{
    public string ProjectionName => "whyce.business.billing.adjustment";

    public string[] EventTypes =>
    [
        "whyce.business.billing.adjustment.created",
        "whyce.business.billing.adjustment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAdjustmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AdjustmentReadModel
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
