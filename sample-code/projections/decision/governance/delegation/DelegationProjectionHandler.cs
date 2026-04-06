using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Delegation;

public sealed class DelegationProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.delegation";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.delegation.created",
        "whyce.decision.governance.delegation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDelegationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DelegationReadModel
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
