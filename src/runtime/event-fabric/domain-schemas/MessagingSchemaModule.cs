using Whycespace.Shared.Contracts.Events.Content.Interaction.Messaging;
using DomainEvents = Whycespace.Domain.ContentSystem.Interaction.Messaging;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/interaction/messaging domain. Binds domain
/// event CLR types to <see cref="EventSchemaRegistry"/> and registers payload
/// mappers that project domain events into shared-contract schemas for Kafka.
/// </summary>
public sealed class MessagingSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MessageSentEvent", EventVersion.Default,
            typeof(DomainEvents.MessageSentEvent), typeof(MessageSentEventSchema));
        sink.RegisterSchema("MessageDeliveredEvent", EventVersion.Default,
            typeof(DomainEvents.MessageDeliveredEvent), typeof(MessageDeliveredEventSchema));
        sink.RegisterSchema("MessageReadEvent", EventVersion.Default,
            typeof(DomainEvents.MessageReadEvent), typeof(MessageReadEventSchema));
        sink.RegisterSchema("MessageEditedEvent", EventVersion.Default,
            typeof(DomainEvents.MessageEditedEvent), typeof(MessageEditedEventSchema));
        sink.RegisterSchema("MessageRetractedEvent", EventVersion.Default,
            typeof(DomainEvents.MessageRetractedEvent), typeof(MessageRetractedEventSchema));

        sink.RegisterPayloadMapper("MessageSentEvent", e =>
        {
            var evt = (DomainEvents.MessageSentEvent)e;
            return new MessageSentEventSchema(
                evt.AggregateId.Value, evt.MessageId.Value,
                evt.ConversationRef, evt.SenderRef, evt.Body, evt.SentAt.Value);
        });

        sink.RegisterPayloadMapper("MessageDeliveredEvent", e =>
        {
            var evt = (DomainEvents.MessageDeliveredEvent)e;
            return new MessageDeliveredEventSchema(
                evt.AggregateId.Value, evt.MessageId.Value, evt.RecipientRef, evt.DeliveredAt.Value);
        });

        sink.RegisterPayloadMapper("MessageReadEvent", e =>
        {
            var evt = (DomainEvents.MessageReadEvent)e;
            return new MessageReadEventSchema(
                evt.AggregateId.Value, evt.MessageId.Value, evt.RecipientRef, evt.ReadAt.Value);
        });

        sink.RegisterPayloadMapper("MessageEditedEvent", e =>
        {
            var evt = (DomainEvents.MessageEditedEvent)e;
            return new MessageEditedEventSchema(
                evt.AggregateId.Value, evt.MessageId.Value, evt.Body, evt.EditedAt.Value);
        });

        sink.RegisterPayloadMapper("MessageRetractedEvent", e =>
        {
            var evt = (DomainEvents.MessageRetractedEvent)e;
            return new MessageRetractedEventSchema(
                evt.AggregateId.Value, evt.MessageId.Value, evt.RetractedAt.Value);
        });
    }
}
