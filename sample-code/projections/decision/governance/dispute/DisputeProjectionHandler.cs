using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Dispute;

public sealed class DisputeProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.dispute";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.dispute.created",
        "whyce.decision.governance.dispute.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDisputeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DisputeReadModel
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
