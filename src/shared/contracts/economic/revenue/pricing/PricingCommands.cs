using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Revenue.Pricing;

public sealed record DefinePricingCommand(
    Guid PricingId,
    Guid ContractId,
    string Model,
    decimal Price,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => PricingId;
}

public sealed record AdjustPricingCommand(
    Guid PricingId,
    decimal NewPrice,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => PricingId;
}
