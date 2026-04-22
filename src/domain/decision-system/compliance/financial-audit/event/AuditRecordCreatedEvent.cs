using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Compliance.Audit;

public sealed record AuditRecordCreatedEvent(
    AuditRecordId AuditRecordId,
    SourceDomain SourceDomain,
    SourceAggregateId SourceAggregateId,
    SourceEventId SourceEventId,
    AuditType AuditType,
    DocumentRef EvidenceSummary,
    Timestamp RecordedAt) : DomainEvent;
