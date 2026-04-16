using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public sealed record ComplianceCheckInitiatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ComplianceCheckId CheckId, string SubjectRef, string RuleRef, Timestamp InitiatedAt) : DomainEvent;
