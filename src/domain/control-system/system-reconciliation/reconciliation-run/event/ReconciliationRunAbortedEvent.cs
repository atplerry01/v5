using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

public sealed record ReconciliationRunAbortedEvent(
    ReconciliationRunId Id,
    string Reason,
    DateTimeOffset AbortedAt) : DomainEvent;
