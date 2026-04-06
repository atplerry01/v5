namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed record RevenueDistributedEvent(
    Guid RootEntityId,
    IReadOnlyDictionary<Guid, decimal> Distribution) : DomainEvent;
