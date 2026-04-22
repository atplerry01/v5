using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Violation;

public sealed record ViolationResolvedEvent(
    ViolationId ViolationId,
    string Resolution,
    Timestamp ResolvedAt) : DomainEvent;
