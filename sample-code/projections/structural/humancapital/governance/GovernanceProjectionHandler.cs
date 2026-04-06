using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Governance;

public sealed class GovernanceProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.governance";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.governance.created",
        "whyce.structural.humancapital.governance.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGovernanceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GovernanceReadModel
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
