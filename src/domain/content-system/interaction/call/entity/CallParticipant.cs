using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Call;

public sealed class CallParticipant
{
    public string ActorRef { get; }
    public Timestamp JoinedAt { get; }
    public Timestamp? LeftAt { get; private set; }

    private CallParticipant(string actorRef, Timestamp joinedAt)
    {
        ActorRef = actorRef;
        JoinedAt = joinedAt;
    }

    public static CallParticipant Join(string actorRef, Timestamp joinedAt)
    {
        if (string.IsNullOrWhiteSpace(actorRef))
            throw CallErrors.InvalidActor();
        return new CallParticipant(actorRef, joinedAt);
    }

    public void Leave(Timestamp at)
    {
        if (LeftAt.HasValue) throw CallErrors.ParticipantAlreadyLeft();
        LeftAt = at;
    }

    public bool IsActive => !LeftAt.HasValue;
}
