namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed class CanCancelSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status)
    {
        return status == ReservationStatus.Pending;
    }
}
