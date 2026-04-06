using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Charge;

public sealed class ChargeProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.charge";

    public string[] EventTypes =>
    [
        "whyce.business.execution.charge.created",
        "whyce.business.execution.charge.updated"
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
