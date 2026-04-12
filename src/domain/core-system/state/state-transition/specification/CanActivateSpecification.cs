namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TransitionStatus status)
    {
        return status == TransitionStatus.Defined;
    }
}
