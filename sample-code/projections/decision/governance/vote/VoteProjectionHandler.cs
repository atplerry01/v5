using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Vote;

public sealed class VoteProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.vote";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.vote.created",
        "whyce.decision.governance.vote.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IVoteViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new VoteReadModel
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
