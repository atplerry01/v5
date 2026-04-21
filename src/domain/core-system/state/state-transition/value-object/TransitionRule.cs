using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public readonly record struct TransitionRule
{
    public string FromState { get; }
    public string ToState { get; }
    public string TransitionName { get; }

    public TransitionRule(string fromState, string toState, string transitionName)
    {
        Guard.Against(string.IsNullOrWhiteSpace(fromState), "FromState must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(toState), "ToState must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(transitionName), "TransitionName must not be empty.");

        FromState = fromState;
        ToState = toState;
        TransitionName = transitionName;
    }
}
