using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Ledger.Settlement;

public sealed class SettlementProjectionHandler
{
    public string ProjectionName => "whyce.economic.ledger.settlement";

    public string[] EventTypes =>
    [
        "whyce.economic.ledger.settlement.created",
        "whyce.economic.ledger.settlement.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISettlementViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SettlementReadModel
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
