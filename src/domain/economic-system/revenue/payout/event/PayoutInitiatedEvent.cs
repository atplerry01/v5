using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutInitiatedEvent(
    PayoutId PayoutId,
    Guid DistributionId,
    Timestamp InitiatedAt) : DomainEvent;
