using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionConfirmedEvent(
    string DistributionId,
    Timestamp ConfirmedAt) : DomainEvent;
