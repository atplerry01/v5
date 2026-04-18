using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionFailedEvent(
    string DistributionId,
    string Reason,
    Timestamp FailedAt) : DomainEvent;
