using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutCompletedEvent(
    PayoutId PayoutId,
    Timestamp CompletedAt) : DomainEvent;
