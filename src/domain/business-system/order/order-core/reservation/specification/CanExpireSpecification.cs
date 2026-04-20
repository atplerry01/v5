namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status) => status == ReservationStatus.Held;
}
