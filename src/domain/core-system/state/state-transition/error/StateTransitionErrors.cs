using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public static class StateTransitionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("StateTransitionId is required and must not be empty.");

    public static DomainException MissingTransitionRule()
        => new DomainInvariantViolationException("State transition must include a valid transition rule.");

    public static DomainException InvalidStateTransition(TransitionStatus current, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("StateTransition has already been initialized.");
}
