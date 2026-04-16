using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Events.Economic.Compliance.Audit;

namespace Whycespace.Projections.Economic.Compliance.Audit.Reducer;

public static class AuditRecordProjectionReducer
{
    public static AuditRecordReadModel Apply(AuditRecordReadModel state, AuditRecordCreatedEventSchema e) =>
        state with
        {
            AuditRecordId = e.AggregateId,
            SourceDomain = e.SourceDomain,
            SourceAggregateId = e.SourceAggregateId,
            SourceEventId = e.SourceEventId,
            AuditType = e.AuditType,
            EvidenceSummary = e.EvidenceSummary,
            Status = "Draft",
            RecordedAt = e.RecordedAt,
            FinalizedAt = null,
            LastUpdatedAt = e.RecordedAt
        };

    public static AuditRecordReadModel Apply(AuditRecordReadModel state, AuditRecordFinalizedEventSchema e) =>
        state with
        {
            AuditRecordId = e.AggregateId,
            Status = "Finalized",
            FinalizedAt = e.FinalizedAt,
            LastUpdatedAt = e.FinalizedAt
        };
}
