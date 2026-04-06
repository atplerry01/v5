using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Cost;

public sealed class CostProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.cost";

    public string[] EventTypes =>
    [
        "whyce.business.execution.cost.created",
        "whyce.business.execution.cost.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostReadModel
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
