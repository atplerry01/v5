namespace Whycespace.Domain.BusinessSystem.Subscription.Plan;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PlanStatus status)
    {
        return status == PlanStatus.Draft;
    }
}

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(PlanStatus status)
    {
        return status == PlanStatus.Active;
    }
}
