namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(PlanStatus status) => status == PlanStatus.Active;
}
