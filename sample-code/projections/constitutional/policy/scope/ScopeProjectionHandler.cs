using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Scope;

public sealed class ScopeProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.scope";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.scope.created",
        "whyce.constitutional.policy.scope.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IScopeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ScopeReadModel
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
