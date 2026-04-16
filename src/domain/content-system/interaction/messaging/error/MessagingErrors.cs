using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public static class MessagingErrors
{
    public static DomainException InvalidBody() => new("Message body must be non-empty.");
    public static DomainException BodyTooLong(int max) => new($"Message body exceeds {max} characters.");
    public static DomainException InvalidSender() => new("Message sender reference must be non-empty.");
    public static DomainException InvalidConversation() => new("Message conversation reference must be non-empty.");
    public static DomainException InvalidAttachment() => new("Message attachment is invalid.");
    public static DomainException InvalidTransition(MessageStatus from, MessageStatus to) =>
        new($"Illegal message state transition from {from} to {to}.");
    public static DomainException AlreadyRetracted() => new("Message has already been retracted.");
    public static DomainException CannotEditAfterRead() => new("Message cannot be edited after being read.");
    public static DomainException NotSent() => new("Message has not been sent.");
    public static DomainInvariantViolationException SenderMissing() =>
        new("Invariant violated: sent message must have a sender.");
    public static DomainInvariantViolationException ConversationMissing() =>
        new("Invariant violated: message must belong to a conversation.");
}
