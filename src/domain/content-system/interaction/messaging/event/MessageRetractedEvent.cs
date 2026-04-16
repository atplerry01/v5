using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public sealed record MessageRetractedEvent(
    EventId EventId,
    AggregateId AggregateId,
    CorrelationId CorrelationId,
    CausationId CausationId,
    MessageId MessageId,
    Timestamp RetractedAt) : DomainEvent;
