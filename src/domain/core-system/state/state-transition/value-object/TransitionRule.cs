namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public readonly record struct TransitionRule
{
    public string FromState { get; }
    public string ToState { get; }
    public string TransitionName { get; }

    public TransitionRule(string fromState, string toState, string transitionName)
    {
        if (string.IsNullOrWhiteSpace(fromState))
            throw new ArgumentException("FromState must not be empty.", nameof(fromState));

        if (string.IsNullOrWhiteSpace(toState))
            throw new ArgumentException("ToState must not be empty.", nameof(toState));

        if (string.IsNullOrWhiteSpace(transitionName))
            throw new ArgumentException("TransitionName must not be empty.", nameof(transitionName));

        FromState = fromState;
        ToState = toState;
        TransitionName = transitionName;
    }
}
