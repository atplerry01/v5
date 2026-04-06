using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Financialcontrol.ReserveControl;

public sealed class ReserveControlProjectionHandler
{
    public string ProjectionName => "whyce.core.financialcontrol.reserve-control";

    public string[] EventTypes =>
    [
        "whyce.core.financialcontrol.reserve-control.created",
        "whyce.core.financialcontrol.reserve-control.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReserveControlViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReserveControlReadModel
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
