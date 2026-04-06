using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Reservation;

public sealed class ReservationProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.reservation";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.reservation.created",
        "whyce.business.inventory.reservation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReservationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReservationReadModel
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
