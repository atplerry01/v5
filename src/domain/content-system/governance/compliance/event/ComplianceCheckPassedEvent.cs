using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public sealed record ComplianceCheckPassedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ComplianceCheckId CheckId, Timestamp PassedAt) : DomainEvent;
