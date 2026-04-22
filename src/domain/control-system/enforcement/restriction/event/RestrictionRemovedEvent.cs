using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

public sealed record RestrictionRemovedEvent(
    RestrictionId RestrictionId,
    SubjectId SubjectId,
    Reason RemovalReason,
    Timestamp RemovedAt) : DomainEvent;
