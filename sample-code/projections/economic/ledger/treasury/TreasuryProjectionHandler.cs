using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Ledger.Treasury;

public sealed class TreasuryProjectionHandler
{
    public string ProjectionName => "whyce.economic.ledger.treasury";

    public string[] EventTypes =>
    [
        "whyce.economic.ledger.treasury.created",
        "whyce.economic.ledger.treasury.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITreasuryViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TreasuryReadModel
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
