namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed class CanReleaseSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status) => status == ReservationStatus.Held;
}
