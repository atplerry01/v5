namespace Whycespace.Shared.Contracts.Events.Content.Interaction.Messaging;

public sealed record MessageSentEventSchema(
    Guid AggregateId,
    Guid MessageId,
    string ConversationRef,
    string SenderRef,
    string Body,
    DateTimeOffset SentAt);

public sealed record MessageDeliveredEventSchema(
    Guid AggregateId,
    Guid MessageId,
    string RecipientRef,
    DateTimeOffset DeliveredAt);

public sealed record MessageReadEventSchema(
    Guid AggregateId,
    Guid MessageId,
    string RecipientRef,
    DateTimeOffset ReadAt);

public sealed record MessageEditedEventSchema(
    Guid AggregateId,
    Guid MessageId,
    string Body,
    DateTimeOffset EditedAt);

public sealed record MessageRetractedEventSchema(
    Guid AggregateId,
    Guid MessageId,
    DateTimeOffset RetractedAt);
