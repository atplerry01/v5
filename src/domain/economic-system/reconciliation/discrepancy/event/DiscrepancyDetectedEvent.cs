using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public sealed record DiscrepancyDetectedEvent(
    DiscrepancyId DiscrepancyId,
    ProcessReference ProcessReference,
    DiscrepancySource Source,
    Amount ExpectedValue,
    Amount ActualValue,
    Amount Difference,
    Timestamp DetectedAt) : DomainEvent;
