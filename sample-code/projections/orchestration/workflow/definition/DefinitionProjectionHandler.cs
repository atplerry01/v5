using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Definition;

public sealed class DefinitionProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.definition";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.definition.created",
        "whyce.orchestration.workflow.definition.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDefinitionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DefinitionReadModel
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
