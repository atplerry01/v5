namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutScheduledEvent(
    Guid PayoutId,
    Guid RecipientId,
    decimal Amount,
    string CurrencyCode) : DomainEvent;
