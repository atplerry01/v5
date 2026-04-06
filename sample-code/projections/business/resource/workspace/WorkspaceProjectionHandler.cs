using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.Workspace;

public sealed class WorkspaceProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.workspace";

    public string[] EventTypes =>
    [
        "whyce.business.resource.workspace.created",
        "whyce.business.resource.workspace.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IWorkspaceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new WorkspaceReadModel
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
