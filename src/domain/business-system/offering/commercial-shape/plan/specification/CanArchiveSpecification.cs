namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(PlanStatus status) => status != PlanStatus.Archived;
}
