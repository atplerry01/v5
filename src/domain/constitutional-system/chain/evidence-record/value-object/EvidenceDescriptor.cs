namespace Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

public sealed record EvidenceDescriptor(
    Guid CorrelationId,
    Guid AnchorRecordId,
    EvidenceType EvidenceType,
    string ActorId,
    string SubjectId,
    string PolicyHash);
