using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Violation;

public sealed record ViolationAcknowledgedEvent(
    ViolationId ViolationId,
    Timestamp AcknowledgedAt) : DomainEvent;
