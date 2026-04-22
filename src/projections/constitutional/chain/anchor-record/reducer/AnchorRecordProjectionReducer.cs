using Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Events.Constitutional.Chain;

namespace Whycespace.Projections.Constitutional.Chain.AnchorRecord.Reducer;

public static class AnchorRecordProjectionReducer
{
    public static AnchorRecordReadModel Apply(AnchorRecordReadModel state, AnchorRecordCreatedEventSchema e) =>
        state with
        {
            AnchorRecordId = e.AggregateId,
            CorrelationId = e.CorrelationId,
            BlockHash = e.BlockHash,
            EventHash = e.EventHash,
            PreviousBlockHash = e.PreviousBlockHash,
            DecisionHash = e.DecisionHash,
            Sequence = e.Sequence,
            Status = "Created",
            AnchoredAt = e.AnchoredAt
        };

    public static AnchorRecordReadModel Apply(AnchorRecordReadModel state, AnchorRecordSealedEventSchema e) =>
        state with
        {
            Status = "Sealed",
            SealedAt = e.SealedAt
        };
}
