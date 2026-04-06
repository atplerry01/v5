using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Resolution;

public sealed class ResolutionProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.resolution";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.resolution.created",
        "whyce.decision.governance.resolution.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IResolutionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ResolutionReadModel
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
