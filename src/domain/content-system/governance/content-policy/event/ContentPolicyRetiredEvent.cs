using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.ContentPolicy;

public sealed record ContentPolicyRetiredEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ContentPolicyId PolicyId, Timestamp RetiredAt) : DomainEvent;
