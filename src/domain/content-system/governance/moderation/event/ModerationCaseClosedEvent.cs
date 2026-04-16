using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed record ModerationCaseClosedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModerationCaseId CaseId, Timestamp ClosedAt) : DomainEvent;
