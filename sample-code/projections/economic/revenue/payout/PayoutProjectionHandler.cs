using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Payout;

public sealed class PayoutProjectionHandler
{
    public string ProjectionName => "whyce.economic.revenue.payout";

    public string[] EventTypes =>
    [
        "whyce.economic.revenue.payout.created",
        "whyce.economic.revenue.payout.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPayoutViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PayoutReadModel
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
