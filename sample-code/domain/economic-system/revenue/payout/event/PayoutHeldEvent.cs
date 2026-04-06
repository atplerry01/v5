namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutHeldEvent(Guid PayoutId, string Reason) : DomainEvent;
