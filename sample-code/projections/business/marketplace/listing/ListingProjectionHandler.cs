using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.Listing;

public sealed class ListingProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.listing";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.listing.created",
        "whyce.business.marketplace.listing.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IListingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ListingReadModel
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
