namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public sealed class CanPauseSpecification
{
    public bool IsSatisfiedBy(TrackingStatus status)
    {
        return status == TrackingStatus.Active;
    }
}
