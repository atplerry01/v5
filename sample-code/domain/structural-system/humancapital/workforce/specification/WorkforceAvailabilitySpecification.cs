namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed class WorkforceAvailabilitySpecification
{
    public bool IsSatisfiedBy(WorkforceMemberAggregate member) => member.Availability == Availability.Available;
}
