using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Charge;

public sealed class ChargeProjectionHandler
{
    public string ProjectionName => "whyce.economic.transaction.charge";

    public string[] EventTypes =>
    [
        "whyce.economic.transaction.charge.created",
        "whyce.economic.transaction.charge.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IChargeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ChargeReadModel
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
