using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.ContentPolicy;

public sealed record ContentPolicyDraftedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ContentPolicyId PolicyId, string Name, int InitialRevision, string Body, Timestamp DraftedAt) : DomainEvent;
