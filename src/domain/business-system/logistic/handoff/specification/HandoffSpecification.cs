namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public sealed class HandoffSpecification
{
    public bool IsSatisfiedBy(HandoffId id, ActorReference sourceActor, ActorReference targetActor, TransferReference transferReference)
    {
        return id != default
            && sourceActor != default
            && targetActor != default
            && sourceActor != targetActor
            && transferReference != default;
    }
}
