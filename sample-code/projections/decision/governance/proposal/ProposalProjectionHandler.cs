using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Proposal;

public sealed class ProposalProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.proposal";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.proposal.created",
        "whyce.decision.governance.proposal.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IProposalViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ProposalReadModel
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
