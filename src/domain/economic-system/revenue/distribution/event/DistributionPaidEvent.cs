using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionPaidEvent(
    string DistributionId,
    string PayoutId,
    Timestamp PaidAt) : DomainEvent;
