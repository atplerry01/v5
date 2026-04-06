using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Scheduler.Booking;

public sealed class BookingProjectionHandler
{
    public string ProjectionName => "whyce.business.scheduler.booking";

    public string[] EventTypes =>
    [
        "whyce.business.scheduler.booking.created",
        "whyce.business.scheduler.booking.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IBookingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new BookingReadModel
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
