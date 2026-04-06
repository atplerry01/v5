using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Access.Role;

public sealed class RoleProjectionHandler
{
    public string ProjectionName => "whyce.trust.access.role";

    public string[] EventTypes =>
    [
        "whyce.trust.access.role.created",
        "whyce.trust.access.role.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRoleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RoleReadModel
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
