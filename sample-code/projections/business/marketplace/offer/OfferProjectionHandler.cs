using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.Offer;

public sealed class OfferProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.offer";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.offer.created",
        "whyce.business.marketplace.offer.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IOfferViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new OfferReadModel
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
