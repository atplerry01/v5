using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Counterparty;

public sealed class CounterpartyProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.counterparty";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.counterparty.created",
        "whyce.business.agreement.counterparty.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICounterpartyViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CounterpartyReadModel
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
