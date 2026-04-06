using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Access.Session;

public sealed class SessionProjectionHandler
{
    public string ProjectionName => "whyce.trust.access.session";

    public string[] EventTypes =>
    [
        "whyce.trust.access.session.created",
        "whyce.trust.access.session.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISessionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SessionReadModel
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
