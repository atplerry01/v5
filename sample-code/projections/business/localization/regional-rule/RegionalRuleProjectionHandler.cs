using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Localization.RegionalRule;

public sealed class RegionalRuleProjectionHandler
{
    public string ProjectionName => "whyce.business.localization.regional-rule";

    public string[] EventTypes =>
    [
        "whyce.business.localization.regional-rule.created",
        "whyce.business.localization.regional-rule.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRegionalRuleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RegionalRuleReadModel
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
