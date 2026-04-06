namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed class AssignmentCapacitySpecification
{
    public bool IsSatisfiedBy(AssignmentAggregate assignment) => !assignment.Status.IsTerminal;
}
