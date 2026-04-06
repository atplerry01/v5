namespace Whycespace.Engines.T2E.Economic.Revenue.Pricing;

public record PricingCommand(string Action, string EntityId, object Payload);

public sealed record CalculatePriceCommand(string Id, decimal Price, string CurrencyCode)
    : PricingCommand("Calculate", Id, null!);
