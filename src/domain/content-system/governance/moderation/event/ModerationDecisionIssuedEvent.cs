using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed record ModerationDecisionIssuedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModerationCaseId CaseId, ModerationDecision Decision, string DeciderRef, string Rationale, Timestamp DecidedAt) : DomainEvent;
