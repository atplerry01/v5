using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Kernel.Domain;

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
///
/// phase1.5-S5.2.4 / HC-1 (OUTBOX-SNAPSHOT-FRESHNESS-01): adds the
/// <c>LastUpdatedAt</c> field and the <c>IsFresh</c> read-time
/// predicate. Closes H19: a dead <c>OutboxDepthSampler</c> would
/// otherwise freeze the snapshot at its last value, and the consumer
/// (<c>PostgresOutboxAdapter.EnqueueAsync</c>) would treat it as
/// authoritative forever — silently corrupting PC-3 refusal decisions.
/// The new field uses the same release-ordering discipline as the
/// other three (<c>Volatile.Write</c> at the end of <c>Publish</c>),
/// so a reader that sees <c>HasObservation == true</c> is guaranteed
/// to also see a corresponding <c>LastUpdatedAt</c>. Freshness is
/// evaluated at read time only — there is no background invalidation
/// or timer.
///
/// IClock is constructor-injected so the publish timestamp is sourced
/// from the canonical Whycespace clock, never <c>DateTime.UtcNow</c>.
/// </summary>
public sealed class OutboxDepthSnapshot : IOutboxDepthSnapshot
{
    private readonly IClock _clock;
    private long _depth;
    private double _oldestPendingAgeSeconds;
    private bool _hasObservation;
    // phase1.5-S5.2.4 / HC-1: stored as UTC ticks (long) so it can be
    // mutated through Volatile.Write the same way the other scalars
    // are. DateTimeOffset itself is a struct without a Volatile.Write
    // overload; ticks round-trip preserves the wall-clock value
    // exactly. The runtime IClock always returns UTC, so storing
    // ticks alone is sufficient.
    private long _lastUpdatedAtUtcTicks;

    public OutboxDepthSnapshot(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
    }

    public long CurrentDepth => Volatile.Read(ref _depth);

    public double OldestPendingAgeSeconds => Volatile.Read(ref _oldestPendingAgeSeconds);

    public bool HasObservation => Volatile.Read(ref _hasObservation);

    public DateTimeOffset LastUpdatedAt
    {
        get
        {
            var ticks = Volatile.Read(ref _lastUpdatedAtUtcTicks);
            return ticks == 0
                ? DateTimeOffset.MinValue
                : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    public void Publish(long depth, double oldestPendingAgeSeconds)
    {
        Volatile.Write(ref _depth, depth);
        Volatile.Write(ref _oldestPendingAgeSeconds, oldestPendingAgeSeconds);
        // phase1.5-S5.2.4 / HC-1: stamp from the canonical IClock
        // before flipping HasObservation, so a reader seeing
        // HasObservation == true is guaranteed to also see a
        // corresponding LastUpdatedAt via the release-store ordering
        // of the surrounding Volatile.Write calls.
        Volatile.Write(ref _lastUpdatedAtUtcTicks, _clock.UtcNow.UtcTicks);
        Volatile.Write(ref _hasObservation, true);
    }

    public bool IsFresh(DateTimeOffset now, int maxAgeSeconds)
    {
        if (!HasObservation) return false;
        var age = (now - LastUpdatedAt).TotalSeconds;
        return age <= maxAgeSeconds;
    }
}
