using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;

public sealed record ExecutionControlSignalIssuedEvent(
    ExecutionControlId Id,
    string JobInstanceId,
    ControlSignal Signal,
    string ActorId,
    DateTimeOffset IssuedAt) : DomainEvent;
