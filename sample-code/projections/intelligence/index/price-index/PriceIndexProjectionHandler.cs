using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Index.PriceIndex;

public sealed class PriceIndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.index.price-index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.index.price-index.created",
        "whyce.intelligence.index.price-index.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPriceIndexViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PriceIndexReadModel
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
