using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.Catalog;

public sealed class CatalogProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.catalog";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.catalog.created",
        "whyce.business.marketplace.catalog.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICatalogViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CatalogReadModel
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
