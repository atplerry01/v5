using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

public sealed record ParticipantRegisteredEvent(
    Guid ParticipantId,
    Guid IdentityId,
    Guid ParticipantTypeId,
    string ParticipantType,
    Guid EntryLevelId,
    string EntryLevel
) : DomainEvent;
