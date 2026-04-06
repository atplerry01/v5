using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.Reservation;

public sealed class ReservationProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.reservation";

    public string[] EventTypes =>
    [
        "whyce.business.resource.reservation.created",
        "whyce.business.resource.reservation.updated"
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
