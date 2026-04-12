namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public static class StateTransitionErrors
{
    public static InvalidOperationException MissingId() =>
        new("StateTransitionId is required and must not be empty.");

    public static InvalidOperationException MissingTransitionRule() =>
        new("State transition must include a valid transition rule.");

    public static InvalidOperationException InvalidStateTransition(TransitionStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
