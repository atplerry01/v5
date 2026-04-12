using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

public sealed record AuditRecordCreatedEvent(
    AuditRecordId AuditRecordId,
    SourceDomain SourceDomain,
    SourceAggregateId SourceAggregateId,
    SourceEventId SourceEventId,
    AuditType AuditType,
    EvidenceSummary EvidenceSummary,
    Timestamp RecordedAt) : DomainEvent;