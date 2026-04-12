namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(TransitionStatus status)
    {
        return status == TransitionStatus.Active;
    }
}
