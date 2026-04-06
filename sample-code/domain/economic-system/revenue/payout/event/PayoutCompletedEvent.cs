namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutCompletedEvent(Guid PayoutId) : DomainEvent;
