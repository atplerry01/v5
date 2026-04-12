namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public sealed class HasValidActorsSpecification
{
    public bool IsSatisfiedBy(ActorReference sourceActor, ActorReference targetActor)
    {
        return sourceActor != default && targetActor != default && sourceActor != targetActor;
    }
}
