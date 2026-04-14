using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Reserve;

namespace Whycespace.Projections.Economic.Capital.Reserve.Reducer;

public static class CapitalReserveProjectionReducer
{
    public static CapitalReserveReadModel Apply(CapitalReserveReadModel state, ReserveCreatedEventSchema e) =>
        state with
        {
            ReserveId = e.AggregateId,
            AccountId = e.AccountId,
            Amount = e.ReservedAmount,
            Currency = e.Currency,
            Status = "Active",
            ReservedAt = e.ReservedAt,
            ExpiresAt = e.ExpiresAt
        };

    public static CapitalReserveReadModel Apply(CapitalReserveReadModel state, ReserveReleasedEventSchema e) =>
        state with
        {
            ReserveId = e.AggregateId,
            Status = "Released"
        };

    public static CapitalReserveReadModel Apply(CapitalReserveReadModel state, ReserveExpiredEventSchema e) =>
        state with
        {
            ReserveId = e.AggregateId,
            Status = "Expired"
        };
}
