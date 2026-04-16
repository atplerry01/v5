using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public sealed record ComplianceCheckFailedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ComplianceCheckId CheckId, string Reason, Timestamp FailedAt) : DomainEvent;
