using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Obligation;

namespace Whycespace.Projections.Economic.Ledger.Obligation.Reducer;

public static class ObligationProjectionReducer
{
    public static ObligationReadModel Apply(ObligationReadModel state, ObligationCreatedEventSchema e) =>
        state with
        {
            ObligationId = e.AggregateId,
            CounterpartyId = e.CounterpartyId,
            Type = e.Type,
            Amount = e.Amount,
            Currency = e.Currency,
            Status = "Pending"
        };

    public static ObligationReadModel Apply(ObligationReadModel state, ObligationFulfilledEventSchema e) =>
        state with
        {
            ObligationId = e.AggregateId,
            SettlementId = e.SettlementId,
            Status = "Fulfilled"
        };

    public static ObligationReadModel Apply(ObligationReadModel state, ObligationCancelledEventSchema e) =>
        state with
        {
            ObligationId = e.AggregateId,
            CancellationReason = e.Reason,
            Status = "Cancelled"
        };
}
