namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PlanStatus status) => status == PlanStatus.Draft;
}
