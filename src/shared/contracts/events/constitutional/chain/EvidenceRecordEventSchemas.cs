namespace Whycespace.Shared.Contracts.Events.Constitutional.Chain;

public sealed record EvidenceRecordCreatedEventSchema(
    Guid AggregateId,
    Guid CorrelationId,
    Guid AnchorRecordId,
    string EvidenceType,
    string ActorId,
    string SubjectId,
    string PolicyHash,
    DateTimeOffset RecordedAt);

public sealed record EvidenceRecordArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
