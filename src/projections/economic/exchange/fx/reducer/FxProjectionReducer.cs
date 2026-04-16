using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Events.Economic.Exchange.Fx;

namespace Whycespace.Projections.Economic.Exchange.Fx.Reducer;

public static class FxProjectionReducer
{
    // P-FX-REGISTEREDAT-01 remediation: FxPairRegisteredEventSchema carries
    // no timestamp field, so the enclosing envelope's Timestamp is the
    // authoritative registration moment. Handler threads it through as
    // `registeredAt`. The prior code path actively wrote DateTimeOffset.MinValue
    // on first apply, which is the 0001-01-01 sentinel that surfaced in the
    // /api/exchange/fx/{id} response.
    public static FxReadModel Apply(
        FxReadModel state,
        FxPairRegisteredEventSchema e,
        DateTimeOffset registeredAt) =>
        state with
        {
            FxId = e.AggregateId,
            BaseCurrency = e.BaseCurrency,
            QuoteCurrency = e.QuoteCurrency,
            Status = "Defined",
            RegisteredAt = registeredAt,
            LastUpdatedAt = registeredAt
        };

    public static FxReadModel Apply(FxReadModel state, FxPairActivatedEventSchema e) =>
        state with
        {
            FxId = e.AggregateId,
            Status = "Active",
            LastUpdatedAt = e.ActivatedAt
        };

    public static FxReadModel Apply(FxReadModel state, FxPairDeactivatedEventSchema e) =>
        state with
        {
            FxId = e.AggregateId,
            Status = "Deactivated",
            LastUpdatedAt = e.DeactivatedAt
        };
}
