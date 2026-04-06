using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Access;

public sealed class AccessProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.access";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.access.created",
        "whyce.constitutional.policy.access.updated"
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
