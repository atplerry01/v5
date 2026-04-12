using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public sealed record DiscrepancyResolvedEvent(
    DiscrepancyId DiscrepancyId,
    string Resolution) : DomainEvent;
