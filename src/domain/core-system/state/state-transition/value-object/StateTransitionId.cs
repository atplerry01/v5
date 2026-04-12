namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public readonly record struct StateTransitionId
{
    public Guid Value { get; }

    public StateTransitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StateTransitionId cannot be empty.", nameof(value));

        Value = value;
    }
}
