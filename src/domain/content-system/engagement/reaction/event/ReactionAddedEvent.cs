using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Reaction;

public sealed record ReactionAddedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ReactionId ReactionId, string TargetRef, string ActorRef, ReactionKind Kind, Timestamp AddedAt) : DomainEvent;
