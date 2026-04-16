using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public sealed class Participant
{
    public string ParticipantRef { get; }
    public Timestamp JoinedAt { get; }
    public Timestamp? LeftAt { get; private set; }

    private Participant(string participantRef, Timestamp joinedAt)
    {
        ParticipantRef = participantRef;
        JoinedAt = joinedAt;
    }

    public static Participant Join(string participantRef, Timestamp joinedAt)
    {
        if (string.IsNullOrWhiteSpace(participantRef))
            throw ConversationErrors.InvalidParticipant();
        return new Participant(participantRef, joinedAt);
    }

    public void Leave(Timestamp leftAt)
    {
        if (LeftAt.HasValue) throw ConversationErrors.ParticipantAlreadyLeft();
        LeftAt = leftAt;
    }

    public bool IsActive => !LeftAt.HasValue;
}
