namespace Whycespace.Shared.Contracts.Events.Economic.Compliance.Audit;

public sealed record AuditRecordCreatedEventSchema(
    Guid AggregateId,
    string SourceDomain,
    Guid SourceAggregateId,
    Guid SourceEventId,
    string AuditType,
    string EvidenceSummary,
    DateTimeOffset RecordedAt);

public sealed record AuditRecordFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);
