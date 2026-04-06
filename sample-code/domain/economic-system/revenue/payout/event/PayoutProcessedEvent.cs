namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutProcessedEvent(Guid PayoutId, decimal Amount) : DomainEvent;
