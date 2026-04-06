using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Access.Authorization;

public sealed class AuthorizationProjectionHandler
{
    public string ProjectionName => "whyce.trust.access.authorization";

    public string[] EventTypes =>
    [
        "whyce.trust.access.authorization.created",
        "whyce.trust.access.authorization.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAuthorizationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AuthorizationReadModel
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
