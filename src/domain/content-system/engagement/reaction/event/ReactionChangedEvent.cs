using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Reaction;

public sealed record ReactionChangedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ReactionId ReactionId, ReactionKind Kind, Timestamp ChangedAt) : DomainEvent;
