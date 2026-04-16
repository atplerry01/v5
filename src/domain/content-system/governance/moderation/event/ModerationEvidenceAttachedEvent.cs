using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed record ModerationEvidenceAttachedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModerationCaseId CaseId, Guid EvidenceId, string ReporterRef, string Description, Timestamp AttachedAt) : DomainEvent;
