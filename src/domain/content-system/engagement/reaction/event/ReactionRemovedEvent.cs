using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Reaction;

public sealed record ReactionRemovedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ReactionId ReactionId, Timestamp RemovedAt) : DomainEvent;
