using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public sealed record ParticipantLeftEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ConversationId ConversationId, string ParticipantRef, Timestamp LeftAt) : DomainEvent;
