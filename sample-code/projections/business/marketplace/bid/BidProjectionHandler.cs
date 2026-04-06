using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.Bid;

public sealed class BidProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.bid";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.bid.created",
        "whyce.business.marketplace.bid.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IBidViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new BidReadModel
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
