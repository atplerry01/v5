using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutRequestedEvent(
    string PayoutId,
    string DistributionId,
    string IdempotencyKey,
    Timestamp RequestedAt) : DomainEvent;
