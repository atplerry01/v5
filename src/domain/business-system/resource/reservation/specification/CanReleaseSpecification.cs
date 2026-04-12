namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed class CanReleaseSpecification
{
    public bool IsSatisfiedBy(ReservationStatus status)
    {
        return status == ReservationStatus.Confirmed;
    }
}
