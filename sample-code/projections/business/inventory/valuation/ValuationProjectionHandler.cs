using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Valuation;

public sealed class ValuationProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.valuation";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.valuation.created",
        "whyce.business.inventory.valuation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IValuationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ValuationReadModel
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
