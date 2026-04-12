using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public sealed record ReconciliationTriggeredEvent(
    ProcessId ProcessId,
    SourceReference LedgerReference,
    SourceReference ObservedReference,
    Timestamp TriggeredAt) : DomainEvent;
