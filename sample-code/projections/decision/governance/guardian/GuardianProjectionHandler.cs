using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Guardian;

public sealed class GuardianProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.guardian";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.guardian.created",
        "whyce.decision.governance.guardian.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGuardianViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GuardianReadModel
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
