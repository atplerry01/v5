using Whycespace.Shared.Contracts.Content.Interaction.Messaging;
using Whycespace.Shared.Contracts.Events.Content.Interaction.Messaging;

namespace Whycespace.Projections.Content.Interaction.Messaging.Reducer;

/// <summary>
/// Pure state reducer for the Message read model. No I/O, no side effects.
/// </summary>
public static class MessageProjectionReducer
{
    public static MessageReadModel Apply(MessageReadModel state, MessageSentEventSchema e)
        => state with
        {
            ConversationRef = e.ConversationRef,
            SenderRef = e.SenderRef,
            Body = e.Body,
            Status = "sent",
            SentAt = e.SentAt
        };

    public static MessageReadModel Apply(MessageReadModel state, MessageDeliveredEventSchema e)
        => state with { Status = "delivered", LastTransitionedAt = e.DeliveredAt, LastRecipientRef = e.RecipientRef };

    public static MessageReadModel Apply(MessageReadModel state, MessageReadEventSchema e)
        => state with { Status = "read", LastTransitionedAt = e.ReadAt, LastRecipientRef = e.RecipientRef };

    public static MessageReadModel Apply(MessageReadModel state, MessageEditedEventSchema e)
        => state with { Body = e.Body, LastTransitionedAt = e.EditedAt };

    public static MessageReadModel Apply(MessageReadModel state, MessageRetractedEventSchema e)
        => state with { Status = "retracted", LastTransitionedAt = e.RetractedAt };
}
