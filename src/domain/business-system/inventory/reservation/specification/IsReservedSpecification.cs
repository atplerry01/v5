namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public sealed class IsReservedSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status)
    {
        return status == ReservationStatus.Reserved;
    }
}
