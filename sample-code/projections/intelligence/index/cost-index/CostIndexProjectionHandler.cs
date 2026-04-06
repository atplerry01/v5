using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Index.CostIndex;

public sealed class CostIndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.index.cost-index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.index.cost-index.created",
        "whyce.intelligence.index.cost-index.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostIndexViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostIndexReadModel
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
