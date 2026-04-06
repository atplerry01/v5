using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Portfolio.Portfolio;

public sealed class PortfolioProjectionHandler
{
    public string ProjectionName => "whyce.business.portfolio.portfolio";

    public string[] EventTypes =>
    [
        "whyce.business.portfolio.portfolio.created",
        "whyce.business.portfolio.portfolio.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPortfolioViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PortfolioReadModel
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
