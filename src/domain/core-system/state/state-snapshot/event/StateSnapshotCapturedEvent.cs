namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public sealed record StateSnapshotCapturedEvent(StateSnapshotId SnapshotId, SnapshotDescriptor Descriptor);
