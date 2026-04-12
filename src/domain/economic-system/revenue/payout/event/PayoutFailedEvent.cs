using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutFailedEvent(
    PayoutId PayoutId,
    string Reason,
    Timestamp FailedAt) : DomainEvent;
