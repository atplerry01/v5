namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TrackingStatus status)
    {
        return status == TrackingStatus.Created;
    }
}
