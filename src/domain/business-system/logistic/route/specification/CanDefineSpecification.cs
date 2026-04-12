namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public sealed class CanDefineSpecification
{
    public bool IsSatisfiedBy(RouteStatus status)
    {
        return status == RouteStatus.Draft;
    }
}
