using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Pool;

namespace Whycespace.Projections.Economic.Capital.Pool.Reducer;

public static class CapitalPoolProjectionReducer
{
    public static CapitalPoolReadModel Apply(CapitalPoolReadModel state, PoolCreatedEventSchema e) =>
        state with
        {
            PoolId = e.AggregateId,
            Currency = e.Currency,
            TotalCapital = 0m,
            CreatedAt = e.CreatedAt
        };

    public static CapitalPoolReadModel Apply(CapitalPoolReadModel state, CapitalAggregatedEventSchema e) =>
        state with
        {
            PoolId = e.AggregateId,
            TotalCapital = e.NewPoolTotal
        };

    public static CapitalPoolReadModel Apply(CapitalPoolReadModel state, CapitalReducedEventSchema e) =>
        state with
        {
            PoolId = e.AggregateId,
            TotalCapital = e.NewPoolTotal
        };
}
