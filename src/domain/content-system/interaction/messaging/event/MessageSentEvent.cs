using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public sealed record MessageSentEvent(
    EventId EventId,
    AggregateId AggregateId,
    CorrelationId CorrelationId,
    CausationId CausationId,
    MessageId MessageId,
    string ConversationRef,
    string SenderRef,
    string Body,
    Timestamp SentAt) : DomainEvent;
