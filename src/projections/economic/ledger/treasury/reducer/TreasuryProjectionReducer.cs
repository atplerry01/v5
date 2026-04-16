using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Treasury;

namespace Whycespace.Projections.Economic.Ledger.Treasury.Reducer;

public static class TreasuryProjectionReducer
{
    public static TreasuryReadModel Apply(TreasuryReadModel state, TreasuryCreatedEventSchema e) =>
        state with
        {
            TreasuryId = e.AggregateId,
            Currency = e.Currency,
            Balance = 0m,
            Status = "Active"
        };

    public static TreasuryReadModel Apply(TreasuryReadModel state, TreasuryFundAllocatedEventSchema e) =>
        state with
        {
            TreasuryId = e.AggregateId,
            Balance = e.NewBalance
        };

    public static TreasuryReadModel Apply(TreasuryReadModel state, TreasuryFundReleasedEventSchema e) =>
        state with
        {
            TreasuryId = e.AggregateId,
            Balance = e.NewBalance
        };
}
