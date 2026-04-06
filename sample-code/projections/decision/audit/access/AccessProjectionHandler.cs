using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Audit.Access;

public sealed class AccessProjectionHandler
{
    public string ProjectionName => "whyce.decision.audit.access";

    public string[] EventTypes =>
    [
        "whyce.decision.audit.access.created",
        "whyce.decision.audit.access.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAccessViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AccessReadModel
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
