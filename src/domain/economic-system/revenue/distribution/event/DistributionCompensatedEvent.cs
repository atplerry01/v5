using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionCompensatedEvent(
    string DistributionId,
    string OriginalPayoutId,
    string CompensatingJournalId,
    Timestamp CompensatedAt) : DomainEvent;
