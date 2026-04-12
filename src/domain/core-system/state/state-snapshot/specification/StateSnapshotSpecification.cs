namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public sealed class CanVerifySpecification
{
    public bool IsSatisfiedBy(SnapshotStatus status)
    {
        return status == SnapshotStatus.Captured;
    }
}

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(SnapshotStatus status)
    {
        return status == SnapshotStatus.Verified;
    }
}
