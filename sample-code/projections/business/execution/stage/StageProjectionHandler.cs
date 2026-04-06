using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Stage;

public sealed class StageProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.stage";

    public string[] EventTypes =>
    [
        "whyce.business.execution.stage.created",
        "whyce.business.execution.stage.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStageViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StageReadModel
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
