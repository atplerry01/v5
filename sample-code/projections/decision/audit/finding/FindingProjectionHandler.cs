using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Audit.Finding;

public sealed class FindingProjectionHandler
{
    public string ProjectionName => "whyce.decision.audit.finding";

    public string[] EventTypes =>
    [
        "whyce.decision.audit.finding.created",
        "whyce.decision.audit.finding.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IFindingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new FindingReadModel
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
