using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Charter;

public sealed class CharterProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.charter";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.charter.created",
        "whyce.decision.governance.charter.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICharterViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CharterReadModel
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
