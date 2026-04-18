using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutCompensationRequestedEvent(
    string PayoutId,
    string DistributionId,
    string IdempotencyKey,
    string Reason,
    Timestamp RequestedAt) : DomainEvent;
