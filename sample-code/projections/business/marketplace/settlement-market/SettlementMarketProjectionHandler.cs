using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.SettlementMarket;

public sealed class SettlementMarketProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.settlement-market";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.settlement-market.created",
        "whyce.business.marketplace.settlement-market.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISettlementMarketViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SettlementMarketReadModel
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
