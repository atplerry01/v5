using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.GovernanceRecord;

public sealed class GovernanceRecordProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.governance-record";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.governance-record.created",
        "whyce.decision.governance.governance-record.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGovernanceRecordViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GovernanceRecordReadModel
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
