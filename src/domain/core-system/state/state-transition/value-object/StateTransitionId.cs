using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public readonly record struct StateTransitionId
{
    public Guid Value { get; }

    public StateTransitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StateTransitionId cannot be empty.");
        Value = value;
    }
}
