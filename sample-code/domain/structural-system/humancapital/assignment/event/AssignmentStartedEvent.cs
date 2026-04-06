using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed record AssignmentStartedEvent(
    Guid AssignmentId
) : DomainEvent;
