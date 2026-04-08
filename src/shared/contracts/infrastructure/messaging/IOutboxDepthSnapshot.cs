namespace Whyce.Shared.Contracts.Infrastructure.Messaging;

/// <summary>
/// phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): shared seam by which the
/// periodic <c>OutboxDepthSampler</c> publishes the latest pending-row
/// count and oldest-pending-age, and by which
/// <c>PostgresOutboxAdapter.EnqueueAsync</c> reads it for the
/// high-water-mark check.
///
/// The seam exists so the enqueue path never issues a fresh
/// <c>COUNT(*)</c> per call (which would defeat the purpose of bounding
/// the outbox under load — the COUNT itself would become the
/// bottleneck). Staleness is bounded by
/// <c>OutboxOptions.SamplingIntervalSeconds</c>; the worst-case
/// admission slop is therefore one sampler tick.
/// </summary>
public interface IOutboxDepthSnapshot
{
    /// <summary>
    /// Latest sampled count of rows where
    /// <c>status IN ('pending','failed')</c>. Returns 0 until the
    /// sampler has run at least once (the safe initial value — a
    /// just-started host has not yet observed any saturation).
    /// </summary>
    long CurrentDepth { get; }

    /// <summary>
    /// Latest sampled age, in seconds, of the oldest pending row.
    /// Returns 0 when no pending rows exist or when the sampler has
    /// not yet run.
    /// </summary>
    double OldestPendingAgeSeconds { get; }

    /// <summary>
    /// True once the sampler has published at least one observation.
    /// The adapter uses this to distinguish "no data yet" (admit, do
    /// not refuse) from "depth observed below the watermark" (admit).
    /// </summary>
    bool HasObservation { get; }

    /// <summary>
    /// Sampler-side write API. Publishes the latest observation
    /// atomically. Called only by <c>OutboxDepthSampler</c>.
    /// </summary>
    void Publish(long depth, double oldestPendingAgeSeconds);
}
