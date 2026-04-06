namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

/// <summary>
/// Topic: whyce.economic.pricing.calculated
/// Command: PriceCalculateCommand
/// </summary>
public sealed record PriceCalculatedEvent(
    Guid PricingId,
    decimal Price,
    string Currency) : DomainEvent;
