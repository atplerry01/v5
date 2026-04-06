using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Chain.Ledger;

public sealed class LedgerProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.chain.ledger";

    public string[] EventTypes =>
    [
        "whyce.constitutional.chain.ledger.created",
        "whyce.constitutional.chain.ledger.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILedgerViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LedgerReadModel
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
