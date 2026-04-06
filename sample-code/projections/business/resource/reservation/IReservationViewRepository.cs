namespace Whycespace.Projections.Business.Resource.Reservation;

public interface IReservationViewRepository
{
    Task SaveAsync(ReservationReadModel model, CancellationToken ct = default);
    Task<ReservationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
