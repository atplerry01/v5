using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public sealed record ConversationRenamedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ConversationId ConversationId, string Topic, Timestamp RenamedAt) : DomainEvent;
