namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed class CanConfirmSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status)
    {
        return status == ReservationStatus.Pending;
    }
}
