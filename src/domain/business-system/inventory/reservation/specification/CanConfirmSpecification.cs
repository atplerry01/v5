namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public sealed class CanConfirmSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status)
    {
        return status == ReservationStatus.Reserved;
    }
}
