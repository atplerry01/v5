using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Access.Permission;

public sealed class PermissionProjectionHandler
{
    public string ProjectionName => "whyce.trust.access.permission";

    public string[] EventTypes =>
    [
        "whyce.trust.access.permission.created",
        "whyce.trust.access.permission.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPermissionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PermissionReadModel
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
