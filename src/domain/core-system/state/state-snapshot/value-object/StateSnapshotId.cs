namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public readonly record struct StateSnapshotId
{
    public Guid Value { get; }

    public StateSnapshotId(Guid value)
    {
        if (value == Guid.Empty)
            throw StateSnapshotErrors.MissingId();

        Value = value;
    }
}
