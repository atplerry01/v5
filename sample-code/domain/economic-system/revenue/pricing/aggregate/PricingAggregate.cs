namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public sealed class PricingAggregate : AggregateRoot
{
    public decimal CalculatedPrice { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    public static PricingAggregate Calculate(Guid id, decimal price, string currency)
    {
        var aggregate = new PricingAggregate();
        aggregate.Id = id;
        aggregate.CalculatedPrice = price;
        aggregate.Currency = currency;
        aggregate.RaiseDomainEvent(new PriceCalculatedEvent(id, price, currency));
        return aggregate;
    }
}
