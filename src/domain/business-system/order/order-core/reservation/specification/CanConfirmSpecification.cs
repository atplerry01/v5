namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed class CanConfirmSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status) => status == ReservationStatus.Held;
}
