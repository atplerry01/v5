using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Rule;

public sealed class RuleProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.rule";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.rule.created",
        "whyce.constitutional.policy.rule.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRuleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RuleReadModel
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
