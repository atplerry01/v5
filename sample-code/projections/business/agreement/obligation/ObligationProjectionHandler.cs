using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Obligation;

public sealed class ObligationProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.obligation";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.obligation.created",
        "whyce.business.agreement.obligation.updated"
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
