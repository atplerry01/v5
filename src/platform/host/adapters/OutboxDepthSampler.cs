using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): periodic sampler that
/// observes the outbox table depth and oldest-pending-row age and
/// publishes both to the shared <see cref="IOutboxDepthSnapshot"/> seam.
/// Also exports the observations as gauges on the existing
/// <c>Whyce.Outbox</c> meter — separate Meter instance, same logical
/// name, so a single scrape sees publisher counters and sampler gauges
/// together.
///
/// Sampling cadence is sized from <see cref="OutboxOptions.SamplingIntervalSeconds"/>.
/// Each tick runs a single bounded SQL probe:
///
///   SELECT COUNT(*),
///          COALESCE(EXTRACT(EPOCH FROM (NOW() - MIN(created_at))), 0)
///     FROM outbox
///    WHERE status IN ('pending','failed')
///
/// Failures are caught and logged — the sampler never crashes the host
/// ($12). On a sampler probe failure the snapshot is *not* updated, so
/// the adapter continues to read the last known good observation;
/// admission is therefore neither over- nor under-tightened by a
/// transient DB hiccup.
/// </summary>
public sealed class OutboxDepthSampler : BackgroundService
{
    // phase1.5-S5.2.1 / PC-3: sampler-side instruments on the canonical
    // Whyce.Outbox meter. Multiple Meter instances sharing a name are
    // collapsed by listeners (OTel, Prometheus exporter, etc.), so the
    // sampler and KafkaOutboxPublisher publish to the same logical
    // surface without coupling their files.
    public static readonly Meter Meter = new("Whyce.Outbox", "1.0");
    private static long _latestDepth;
    private static double _latestOldestAgeSeconds;
    // phase1.5-S5.2.2 / KC-3 (DLQ-OBSERVABILITY-01): outbox-side
    // deadletter row depth. Sampled by the same probe as
    // outbox.depth, but counts only rows where status='deadletter'
    // (the PC-3 high-water-mark intentionally excludes those rows
    // because the watermark is for admission, not for retention).
    // This gauge closes the K-R-02 outbox-side observability gap.
    private static long _latestDeadletterDepth;
    private static readonly ObservableGauge<long> DepthGauge =
        Meter.CreateObservableGauge("outbox.depth", () => Volatile.Read(ref _latestDepth));
    private static readonly ObservableGauge<double> OldestAgeGauge =
        Meter.CreateObservableGauge(
            "outbox.oldest_pending_age_seconds",
            () => Volatile.Read(ref _latestOldestAgeSeconds));
    private static readonly ObservableGauge<long> DeadletterDepthGauge =
        Meter.CreateObservableGauge(
            "outbox.deadletter_depth",
            () => Volatile.Read(ref _latestDeadletterDepth));

    private readonly EventStoreDataSource _dataSource;
    private readonly IOutboxDepthSnapshot _snapshot;
    private readonly TimeSpan _samplingInterval;
    private readonly IWorkerLivenessRegistry _liveness;
    private readonly IClock _clock;
    private readonly ILogger<OutboxDepthSampler>? _logger;

    // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): canonical worker
    // name reported into the IWorkerLivenessRegistry after each
    // successful sampling iteration.
    private const string WorkerName = "outbox-sampler";

    // phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): the sampler now
    // acquires its periodic probe connection from the declared
    // event-store pool, so its activity counts toward the same
    // postgres.pool.* metrics as the rest of the event-store path.
    //
    // phase1.5-S5.2.4 / HC-1 (OUTBOX-SNAPSHOT-FRESHNESS-01): no
    // signature change required here. The sampler still calls
    // _snapshot.Publish(depth, age) — the IClock-based LastUpdatedAt
    // stamping happens inside OutboxDepthSnapshot.Publish itself, so
    // the sampler stays narrowly scoped to its existing role.
    public OutboxDepthSampler(
        EventStoreDataSource dataSource,
        IOutboxDepthSnapshot snapshot,
        OutboxOptions options,
        IWorkerLivenessRegistry liveness,
        IClock clock,
        ILogger<OutboxDepthSampler>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(liveness);
        ArgumentNullException.ThrowIfNull(clock);
        if (options.SamplingIntervalSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.SamplingIntervalSeconds,
                "OutboxOptions.SamplingIntervalSeconds must be at least 1.");

        _dataSource = dataSource;
        _snapshot = snapshot;
        _samplingInterval = TimeSpan.FromSeconds(options.SamplingIntervalSeconds);
        _liveness = liveness;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run the first probe immediately so HasObservation flips to
        // true before the first request arrives in normal startup
        // ordering. Subsequent ticks honor the configured cadence.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SampleOnceAsync(stoppingToken);
                // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): record
                // a successful iteration ONLY on the success path —
                // never inside a catch block. The IClock-sourced
                // timestamp is the worker-side liveness signal that
                // WorkersHealthCheck reads at probe time.
                _liveness.RecordSuccess(WorkerName, _clock.UtcNow);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "OutboxDepthSampler probe failed; snapshot unchanged.");
            }

            try { await Task.Delay(_samplingInterval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task SampleOnceAsync(CancellationToken ct)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName, ct);

        // phase1.5-S5.2.2 / KC-3: single bounded probe sampling both
        // pending+failed depth (PC-3) and deadletter depth (KC-3).
        // The two counts come from a single round-trip via FILTER —
        // no additional pool acquisition relative to PC-3.
        await using var cmd = new NpgsqlCommand(
            """
            SELECT COUNT(*) FILTER (WHERE status IN ('pending','failed'))::bigint,
                   COALESCE(EXTRACT(EPOCH FROM (NOW() - MIN(created_at) FILTER (WHERE status IN ('pending','failed')))), 0)::double precision,
                   COUNT(*) FILTER (WHERE status = 'deadletter')::bigint
              FROM outbox
            """,
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return;

        var depth = reader.GetInt64(0);
        var oldestAgeSeconds = reader.GetDouble(1);
        var deadletterDepth = reader.GetInt64(2);

        Volatile.Write(ref _latestDepth, depth);
        Volatile.Write(ref _latestOldestAgeSeconds, oldestAgeSeconds);
        Volatile.Write(ref _latestDeadletterDepth, deadletterDepth);
        _snapshot.Publish(depth, oldestAgeSeconds);
    }
}
