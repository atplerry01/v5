namespace Whycespace.Domain.StructuralSystem.HumanCapital.Sanction;

public sealed class SanctionApplicabilitySpecification
{
    public bool IsSatisfiedBy(SanctionAggregate sanction) => sanction.IsActive;
}
