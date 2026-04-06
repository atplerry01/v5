using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Sanction;

public sealed class SanctionProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.sanction";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.sanction.created",
        "whyce.decision.governance.sanction.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISanctionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SanctionReadModel
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
