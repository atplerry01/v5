using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed record AssignmentCreatedEvent(
    Guid AssignmentId,
    Guid AssigneeIdentityId,
    Guid ScopeId,
    string ScopeType,
    DateTimeOffset Start,
    DateTimeOffset End
) : DomainEvent;
