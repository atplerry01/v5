using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Compliance.Obligation;

public sealed class ObligationProjectionHandler
{
    public string ProjectionName => "whyce.decision.compliance.obligation";

    public string[] EventTypes =>
    [
        "whyce.decision.compliance.obligation.created",
        "whyce.decision.compliance.obligation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IObligationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ObligationReadModel
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
