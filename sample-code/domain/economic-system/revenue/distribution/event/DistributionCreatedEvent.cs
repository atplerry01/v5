namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionCreatedEvent(Guid DistributionId) : DomainEvent;
