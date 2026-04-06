namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutFailedEvent(Guid PayoutId, string Reason) : DomainEvent;
