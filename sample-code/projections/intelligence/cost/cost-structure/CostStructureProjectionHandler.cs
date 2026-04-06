using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Cost.CostStructure;

public sealed class CostStructureProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.cost.cost-structure";

    public string[] EventTypes =>
    [
        "whyce.intelligence.cost.cost-structure.created",
        "whyce.intelligence.cost.cost-structure.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostStructureViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostStructureReadModel
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
