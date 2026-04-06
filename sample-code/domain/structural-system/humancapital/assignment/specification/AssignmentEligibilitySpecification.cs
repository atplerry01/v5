namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed class AssignmentEligibilitySpecification
{
    public bool IsSatisfiedBy(AssignmentAggregate assignment) => assignment.Status != AssignmentStatus.Failed;
}
