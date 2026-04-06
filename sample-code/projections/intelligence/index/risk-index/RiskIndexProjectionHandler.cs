using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Index.RiskIndex;

public sealed class RiskIndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.index.risk-index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.index.risk-index.created",
        "whyce.intelligence.index.risk-index.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRiskIndexViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RiskIndexReadModel
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
