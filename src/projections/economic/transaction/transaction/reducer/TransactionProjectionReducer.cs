using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Transaction;

namespace Whycespace.Projections.Economic.Transaction.Transaction.Reducer;

public static class TransactionProjectionReducer
{
    public static TransactionReadModel Apply(TransactionReadModel state, TransactionInitiatedEventSchema e) =>
        state with
        {
            TransactionId = e.AggregateId,
            Kind = e.Kind,
            References = e.References,
            Status = "Initiated",
            InitiatedAt = e.InitiatedAt
        };

    public static TransactionReadModel Apply(TransactionReadModel state, TransactionCommittedEventSchema e) =>
        state with
        {
            TransactionId = e.AggregateId,
            Kind = e.Kind,
            References = e.References,
            Status = "Committed",
            CommittedAt = e.CommittedAt
        };

    public static TransactionReadModel Apply(TransactionReadModel state, TransactionFailedEventSchema e) =>
        state with
        {
            TransactionId = e.AggregateId,
            Status = "Failed",
            FailedAt = e.FailedAt,
            FailureReason = e.Reason
        };
}
