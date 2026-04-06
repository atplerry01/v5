using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

public sealed record ParticipantActivatedEvent(
    Guid ParticipantId
) : DomainEvent;
