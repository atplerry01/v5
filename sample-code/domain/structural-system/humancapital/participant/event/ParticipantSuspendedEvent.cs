using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

public sealed record ParticipantSuspendedEvent(
    Guid ParticipantId,
    string Reason
) : DomainEvent;
