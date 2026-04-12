namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public sealed class CanLockSpecification
{
    public bool IsSatisfiedBy(RouteStatus status)
    {
        return status == RouteStatus.Defined;
    }
}
