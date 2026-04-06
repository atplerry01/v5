namespace Whycespace.Projections.Core.State.StateSnapshot;

public sealed record StateSnapshotView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
