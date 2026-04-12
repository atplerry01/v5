namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed record StateTransitionDefinedEvent(
    StateTransitionId TransitionId,
    TransitionRule Rule);
