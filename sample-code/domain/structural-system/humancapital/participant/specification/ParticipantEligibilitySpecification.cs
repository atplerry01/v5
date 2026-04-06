namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

public sealed class ParticipantEligibilitySpecification
{
    public bool IsSatisfiedBy(ParticipantAggregate participant) => participant.IsActive;
}
