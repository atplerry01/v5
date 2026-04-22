using Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Events.Constitutional.Chain;

namespace Whycespace.Projections.Constitutional.Chain.EvidenceRecord.Reducer;

public static class EvidenceRecordProjectionReducer
{
    public static EvidenceRecordReadModel Apply(EvidenceRecordReadModel state, EvidenceRecordCreatedEventSchema e) =>
        state with
        {
            EvidenceRecordId = e.AggregateId,
            CorrelationId = e.CorrelationId,
            AnchorRecordId = e.AnchorRecordId,
            EvidenceType = e.EvidenceType,
            ActorId = e.ActorId,
            SubjectId = e.SubjectId,
            PolicyHash = e.PolicyHash,
            Status = "Active",
            RecordedAt = e.RecordedAt
        };

    public static EvidenceRecordReadModel Apply(EvidenceRecordReadModel state, EvidenceRecordArchivedEventSchema e) =>
        state with
        {
            Status = "Archived",
            ArchivedAt = e.ArchivedAt
        };
}
