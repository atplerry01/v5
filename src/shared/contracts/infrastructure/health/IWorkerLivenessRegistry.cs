namespace Whyce.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): narrow in-process
/// seam by which background workers report a successful loop
/// iteration. The registry is storage-only — no timers, no pruning,
/// no background thread. Liveness is judged at read time by
/// <c>WorkersHealthCheck</c> against
/// <see cref="WorkerHealthOptions.MaxSilenceSeconds"/>.
///
/// Worker names are low-cardinality canonical identifiers; HC-5 uses:
///   - "outbox-sampler"
///   - "kafka-outbox-publisher"
///   - "projection-consumer"
/// </summary>
public interface IWorkerLivenessRegistry
{
    /// <summary>
    /// Worker-side write API. Called from each <c>BackgroundService</c>
    /// after a successful loop iteration. <paramref name="now"/> must
    /// be sourced from <c>IClock.UtcNow</c> at the call site.
    /// </summary>
    void RecordSuccess(string workerName, DateTimeOffset now);

    /// <summary>
    /// Reader-side API. Returns one snapshot per worker name that has
    /// ever been observed by the registry. <paramref name="now"/> is
    /// accepted for parity with other read-time health predicates and
    /// is reserved for future per-snapshot age computation; the
    /// registry itself does not derive any state from it.
    /// </summary>
    IReadOnlyList<WorkerLivenessSnapshot> GetSnapshots(DateTimeOffset now);
}
