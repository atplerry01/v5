using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public readonly record struct StateSnapshotId
{
    public Guid Value { get; }

    public StateSnapshotId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StateSnapshotId cannot be empty.");
        Value = value;
    }
}
