using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Portfolio.Rebalance;

public sealed class RebalanceProjectionHandler
{
    public string ProjectionName => "whyce.business.portfolio.rebalance";

    public string[] EventTypes =>
    [
        "whyce.business.portfolio.rebalance.created",
        "whyce.business.portfolio.rebalance.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRebalanceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RebalanceReadModel
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
