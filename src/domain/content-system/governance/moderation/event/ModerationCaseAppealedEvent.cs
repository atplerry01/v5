using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed record ModerationCaseAppealedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModerationCaseId CaseId, string AppellantRef, string Grounds, Timestamp AppealedAt) : DomainEvent;
