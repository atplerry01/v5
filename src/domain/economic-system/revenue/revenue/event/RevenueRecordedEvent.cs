using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed record RevenueRecordedEvent(
    string RevenueId,
    string SpvId,
    decimal Amount,
    string Currency,
    string SourceRef) : DomainEvent;
