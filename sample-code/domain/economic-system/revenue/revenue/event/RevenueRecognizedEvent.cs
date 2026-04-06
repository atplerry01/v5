namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

/// <summary>
/// Topic: whyce.economic.revenue.recognized
/// Command: RevenueRecognizeCommand
/// </summary>
public sealed record RevenueRecognizedEvent(
    Guid RevenueId,
    decimal Amount,
    string CurrencyCode) : DomainEvent;
