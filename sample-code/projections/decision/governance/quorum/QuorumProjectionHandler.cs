using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Quorum;

public sealed class QuorumProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.quorum";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.quorum.created",
        "whyce.decision.governance.quorum.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IQuorumViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new QuorumReadModel
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
