using Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.Events.Constitutional.Chain;

namespace Whycespace.Projections.Constitutional.Chain.Ledger.Reducer;

public static class LedgerProjectionReducer
{
    public static LedgerReadModel Apply(LedgerReadModel state, LedgerOpenedEventSchema e) =>
        state with
        {
            LedgerId = e.AggregateId,
            LedgerName = e.LedgerName,
            Status = "Open",
            OpenedAt = e.OpenedAt
        };

    public static LedgerReadModel Apply(LedgerReadModel state, LedgerSealedEventSchema e) =>
        state with
        {
            Status = "Sealed",
            SealedAt = e.SealedAt
        };
}
