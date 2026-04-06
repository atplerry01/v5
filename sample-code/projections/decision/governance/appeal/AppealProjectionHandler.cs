using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Appeal;

public sealed class AppealProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.appeal";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.appeal.created",
        "whyce.decision.governance.appeal.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAppealViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AppealReadModel
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
