using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Escalation;

public sealed record ViolationAccumulatedEvent(
    SubjectId SubjectId,
    Guid ViolationId,
    int SeverityWeight,
    ViolationCounter NewCounter,
    Timestamp AccumulatedAt) : DomainEvent;
