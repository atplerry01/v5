using Whyce.Shared.Contracts.Infrastructure.Messaging;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): in-process
/// <see cref="IOutboxDepthSnapshot"/> implementation. Single-instance,
/// singleton-registered. The sampler writes via <see cref="Publish"/>;
/// the adapter reads via the properties.
///
/// Reads and writes are coordinated through <see cref="Volatile"/> on
/// the backing fields — sufficient because (a) the snapshot is a small
/// set of independent scalars, (b) callers tolerate single-tick
/// staleness by design, and (c) introducing a lock would re-create the
/// per-enqueue contention the seam exists to avoid. The
/// <c>_hasObservation</c> flag is set last so a reader that sees
/// <c>HasObservation == true</c> is guaranteed to see a corresponding
/// depth+age pair from the same publish call (release-store ordering
/// via <see cref="Volatile.Write"/>).
/// </summary>
public sealed class OutboxDepthSnapshot : IOutboxDepthSnapshot
{
    private long _depth;
    private double _oldestPendingAgeSeconds;
    private bool _hasObservation;

    public long CurrentDepth => Volatile.Read(ref _depth);

    public double OldestPendingAgeSeconds => Volatile.Read(ref _oldestPendingAgeSeconds);

    public bool HasObservation => Volatile.Read(ref _hasObservation);

    public void Publish(long depth, double oldestPendingAgeSeconds)
    {
        Volatile.Write(ref _depth, depth);
        Volatile.Write(ref _oldestPendingAgeSeconds, oldestPendingAgeSeconds);
        Volatile.Write(ref _hasObservation, true);
    }
}
