using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Events.Economic.Exchange.Rate;

namespace Whycespace.Projections.Economic.Exchange.Rate.Reducer;

public static class ExchangeRateProjectionReducer
{
    public static ExchangeRateReadModel Apply(ExchangeRateReadModel state, ExchangeRateDefinedEventSchema e) =>
        state with
        {
            RateId = e.AggregateId,
            BaseCurrency = e.BaseCurrency,
            QuoteCurrency = e.QuoteCurrency,
            RateValue = e.RateValue,
            EffectiveAt = e.EffectiveAt,
            VersionNumber = e.Version,
            Status = "Defined",
            LastUpdatedAt = e.EffectiveAt
        };

    public static ExchangeRateReadModel Apply(ExchangeRateReadModel state, ExchangeRateActivatedEventSchema e) =>
        state with
        {
            RateId = e.AggregateId,
            Status = "Active",
            LastUpdatedAt = e.ActivatedAt
        };

    public static ExchangeRateReadModel Apply(ExchangeRateReadModel state, ExchangeRateExpiredEventSchema e) =>
        state with
        {
            RateId = e.AggregateId,
            Status = "Expired",
            LastUpdatedAt = e.ExpiredAt
        };
}
