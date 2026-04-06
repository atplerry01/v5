namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

/// <summary>
/// Topic: whyce.economic.revenue.reversed
/// </summary>
public sealed record RevenueReversedEvent(
    Guid RevenueId,
    decimal Amount,
    string CurrencyCode) : DomainEvent;
