namespace Whycespace.Projections.Business.Scheduler.Booking;

public interface IBookingViewRepository
{
    Task SaveAsync(BookingReadModel model, CancellationToken ct = default);
    Task<BookingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
