using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutFailedEvent(
    string PayoutId,
    string DistributionId,
    string Reason,
    Timestamp FailedAt) : DomainEvent;
