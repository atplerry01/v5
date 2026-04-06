namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionExecutedEvent(
    Guid DistributionId,
    decimal Amount,
    string CurrencyCode
) : DomainEvent;
