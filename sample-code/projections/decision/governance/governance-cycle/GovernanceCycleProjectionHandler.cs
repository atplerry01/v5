using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.GovernanceCycle;

public sealed class GovernanceCycleProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.governance-cycle";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.governance-cycle.created",
        "whyce.decision.governance.governance-cycle.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGovernanceCycleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GovernanceCycleReadModel
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
