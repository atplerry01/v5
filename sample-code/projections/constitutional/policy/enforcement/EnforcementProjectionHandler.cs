using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Enforcement;

public sealed class EnforcementProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.enforcement";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.enforcement.created",
        "whyce.constitutional.policy.enforcement.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEnforcementViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EnforcementReadModel
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
