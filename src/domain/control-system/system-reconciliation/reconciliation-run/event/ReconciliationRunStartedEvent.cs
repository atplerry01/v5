using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

public sealed record ReconciliationRunStartedEvent(
    ReconciliationRunId Id,
    DateTimeOffset StartedAt) : DomainEvent;
