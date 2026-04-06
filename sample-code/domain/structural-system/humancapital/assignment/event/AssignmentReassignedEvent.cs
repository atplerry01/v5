using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed record AssignmentReassignedEvent(
    Guid AssignmentId,
    Guid NewAssigneeIdentityId
) : DomainEvent;
