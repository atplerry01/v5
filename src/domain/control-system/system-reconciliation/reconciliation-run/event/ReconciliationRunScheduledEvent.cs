using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

public sealed record ReconciliationRunScheduledEvent(
    ReconciliationRunId Id,
    string Scope) : DomainEvent;
