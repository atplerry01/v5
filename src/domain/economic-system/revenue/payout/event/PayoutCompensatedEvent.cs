using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutCompensatedEvent(
    string PayoutId,
    string DistributionId,
    string IdempotencyKey,
    string CompensatingJournalId,
    Timestamp CompensatedAt) : DomainEvent;
