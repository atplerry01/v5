namespace Whycespace.Domain.CoreSystem.State.SystemState;

/// <summary>
/// Point-in-time snapshot of system state, capturing key health indicators.
/// </summary>
public sealed record SystemSnapshot
{
    public DateTimeOffset CapturedAt { get; }
    public long EventStoreVersion { get; }
    public int ActiveAggregates { get; }
    public string SnapshotHash { get; }

    private SystemSnapshot(DateTimeOffset capturedAt, long eventStoreVersion, int activeAggregates, string snapshotHash)
    {
        CapturedAt = capturedAt;
        EventStoreVersion = eventStoreVersion;
        ActiveAggregates = activeAggregates;
        SnapshotHash = snapshotHash;
    }

    public static SystemSnapshot Capture(DateTimeOffset capturedAt, long eventStoreVersion, int activeAggregates, string snapshotHash) =>
        string.IsNullOrWhiteSpace(snapshotHash)
            ? throw new ArgumentException("Snapshot hash must not be empty.", nameof(snapshotHash))
            : new(capturedAt, eventStoreVersion, activeAggregates, snapshotHash);
}
