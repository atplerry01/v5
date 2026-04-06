using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Authority;

public sealed class AuthorityProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.authority";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.authority.created",
        "whyce.decision.governance.authority.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAuthorityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AuthorityReadModel
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
