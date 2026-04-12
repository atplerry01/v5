using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public sealed record DiscrepancyInvestigatedEvent(
    DiscrepancyId DiscrepancyId) : DomainEvent;
