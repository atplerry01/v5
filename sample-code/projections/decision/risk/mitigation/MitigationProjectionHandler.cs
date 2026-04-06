using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Mitigation;

public sealed class MitigationProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.mitigation";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.mitigation.created",
        "whyce.decision.risk.mitigation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMitigationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MitigationReadModel
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
