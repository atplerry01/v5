using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public sealed record ReconciliationResolvedEvent(
    ProcessId ProcessId) : DomainEvent;
