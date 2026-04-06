using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Scope;

public sealed class ScopeProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.scope";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.scope.created",
        "whyce.decision.governance.scope.updated"
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
