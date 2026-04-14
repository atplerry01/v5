using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Settlement;

namespace Whycespace.Projections.Economic.Transaction.Settlement.Reducer;

public static class SettlementProjectionReducer
{
    public static SettlementReadModel Apply(SettlementReadModel state, SettlementInitiatedEventSchema e) =>
        state with
        {
            SettlementId = e.AggregateId,
            Amount = e.Amount,
            Currency = e.Currency,
            SourceReference = e.SourceReference,
            Provider = e.Provider,
            Status = "Initiated"
        };

    public static SettlementReadModel Apply(SettlementReadModel state, SettlementProcessingStartedEventSchema e) =>
        state with
        {
            SettlementId = e.AggregateId,
            Status = "Processing"
        };

    public static SettlementReadModel Apply(SettlementReadModel state, SettlementCompletedEventSchema e) =>
        state with
        {
            SettlementId = e.AggregateId,
            Status = "Completed",
            ExternalReferenceId = e.ExternalReferenceId
        };

    public static SettlementReadModel Apply(SettlementReadModel state, SettlementFailedEventSchema e) =>
        state with
        {
            SettlementId = e.AggregateId,
            Status = "Failed",
            FailureReason = e.Reason
        };
}
