using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public static class ConversationErrors
{
    public static DomainException InvalidTopic() => new("Conversation topic must be non-empty.");
    public static DomainException TopicTooLong(int max) => new($"Conversation topic exceeds {max} characters.");
    public static DomainException InvalidParticipant() => new("Participant reference must be non-empty.");
    public static DomainException ParticipantAlreadyLeft() => new("Participant has already left.");
    public static DomainException ParticipantAlreadyInConversation() => new("Participant already in conversation.");
    public static DomainException ParticipantNotInConversation() => new("Participant is not in this conversation.");
    public static DomainException AlreadyArchived() => new("Conversation is already archived.");
    public static DomainException CannotMutateArchived() => new("Archived conversations are immutable.");
    public static DomainInvariantViolationException AtLeastOneParticipant() =>
        new("Invariant violated: conversation must have at least one active participant.");
}
