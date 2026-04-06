using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Committee;

public sealed class CommitteeProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.committee";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.committee.created",
        "whyce.decision.governance.committee.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICommitteeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CommitteeReadModel
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
