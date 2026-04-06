namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionClawbackEvent(
    Guid DistributionId,
    Guid TransactionId) : DomainEvent;
