using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionCompensationRequestedEvent(
    string DistributionId,
    string OriginalPayoutId,
    string Reason,
    Timestamp RequestedAt) : DomainEvent;
