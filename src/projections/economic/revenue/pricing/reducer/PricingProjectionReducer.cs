using Whycespace.Shared.Contracts.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Pricing;

namespace Whycespace.Projections.Economic.Revenue.Pricing.Reducer;

public static class PricingProjectionReducer
{
    public static PricingReadModel Apply(PricingReadModel state, PriceDefinedEventSchema e) =>
        state with
        {
            PricingId = e.AggregateId,
            ContractId = e.ContractId,
            Model = e.Model,
            Price = e.Price,
            Currency = e.Currency,
            DefinedAt = e.DefinedAt,
            AdjustmentCount = 0
        };

    public static PricingReadModel Apply(PricingReadModel state, PriceAdjustedEventSchema e) =>
        state with
        {
            Price = e.NewPrice,
            LastAdjustedAt = e.AdjustedAt,
            AdjustmentCount = state.AdjustmentCount + 1
        };
}
