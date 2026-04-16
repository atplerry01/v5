using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed record ModerationCaseOpenedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModerationCaseId CaseId, string TargetRef, string ReporterRef, Timestamp OpenedAt) : DomainEvent;
