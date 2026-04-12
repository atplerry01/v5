namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public static class StateSnapshotErrors
{
    public static InvalidOperationException MissingId()
        => new("StateSnapshotId requires a non-empty Guid.");

    public static InvalidOperationException MissingDescriptor()
        => new("StateSnapshot requires a valid SnapshotDescriptor.");

    public static InvalidOperationException InvalidStateTransition(SnapshotStatus status, string action)
        => new($"Cannot {action} a snapshot in {status} status.");
}
