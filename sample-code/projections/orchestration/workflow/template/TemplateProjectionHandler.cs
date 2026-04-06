using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Template;

public sealed class TemplateProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.template";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.template.created",
        "whyce.orchestration.workflow.template.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITemplateViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TemplateReadModel
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
