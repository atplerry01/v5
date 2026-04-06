namespace Whycespace.Domain.StructuralSystem.HumanCapital.Incentive;

public sealed class IncentiveEligibilitySpecification
{
    public bool IsSatisfiedBy(IncentiveAggregate incentive) => incentive.IsActive;
}
