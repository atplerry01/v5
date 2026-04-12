namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public sealed class CanReleaseSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status)
    {
        return status == ReservationStatus.Reserved;
    }
}
