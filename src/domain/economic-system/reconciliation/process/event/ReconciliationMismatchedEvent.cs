using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public sealed record ReconciliationMismatchedEvent(
    ProcessId ProcessId) : DomainEvent;
