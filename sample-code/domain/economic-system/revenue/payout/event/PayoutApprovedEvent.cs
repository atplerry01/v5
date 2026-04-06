namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutApprovedEvent(Guid PayoutId) : DomainEvent;
