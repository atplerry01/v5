using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Portfolio.Holding;

public sealed class HoldingProjectionHandler
{
    public string ProjectionName => "whyce.business.portfolio.holding";

    public string[] EventTypes =>
    [
        "whyce.business.portfolio.holding.created",
        "whyce.business.portfolio.holding.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IHoldingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new HoldingReadModel
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
