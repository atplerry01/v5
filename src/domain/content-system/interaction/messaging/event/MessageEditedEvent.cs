using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public sealed record MessageEditedEvent(
    EventId EventId,
    AggregateId AggregateId,
    CorrelationId CorrelationId,
    CausationId CausationId,
    MessageId MessageId,
    string Body,
    Timestamp EditedAt) : DomainEvent;
