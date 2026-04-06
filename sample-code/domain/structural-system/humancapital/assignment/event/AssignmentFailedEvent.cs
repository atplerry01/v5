using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed record AssignmentFailedEvent(
    Guid AssignmentId,
    string Reason
) : DomainEvent;
