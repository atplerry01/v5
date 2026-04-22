using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;

public sealed record ExecutionControlSignalOutcomeRecordedEvent(
    ExecutionControlId Id,
    ControlSignalOutcome Outcome) : DomainEvent;
