using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

public sealed record ReconciliationRunCompletedEvent(
    ReconciliationRunId Id,
    int ChecksProcessed,
    int DiscrepanciesFound,
    DateTimeOffset CompletedAt) : DomainEvent;
