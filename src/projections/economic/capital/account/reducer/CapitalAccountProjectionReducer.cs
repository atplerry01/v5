using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Account;

namespace Whycespace.Projections.Economic.Capital.Account.Reducer;

public static class CapitalAccountProjectionReducer
{
    public static CapitalAccountReadModel Apply(CapitalAccountReadModel state, CapitalAccountOpenedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            OwnerId = e.OwnerId,
            Currency = e.Currency,
            TotalBalance = 0m,
            AvailableBalance = 0m,
            ReservedBalance = 0m,
            Status = "Active",
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        };

    public static CapitalAccountReadModel Apply(CapitalAccountReadModel state, CapitalFundedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            TotalBalance = e.NewTotalBalance,
            AvailableBalance = e.NewAvailableBalance
        };

    public static CapitalAccountReadModel Apply(CapitalAccountReadModel state, AccountCapitalAllocatedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            TotalBalance = state.TotalBalance - e.AllocatedAmount,
            AvailableBalance = e.NewAvailableBalance
        };

    public static CapitalAccountReadModel Apply(CapitalAccountReadModel state, AccountCapitalReservedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            AvailableBalance = e.NewAvailableBalance,
            ReservedBalance = e.NewReservedBalance
        };

    public static CapitalAccountReadModel Apply(CapitalAccountReadModel state, AccountReservationReleasedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            AvailableBalance = e.NewAvailableBalance,
            ReservedBalance = e.NewReservedBalance
        };

    public static CapitalAccountReadModel Apply(CapitalAccountReadModel state, CapitalAccountFrozenEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            Status = "Frozen"
        };

    public static CapitalAccountReadModel Apply(CapitalAccountReadModel state, CapitalAccountClosedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            Status = "Closed",
            LastUpdatedAt = e.ClosedAt
        };
}
