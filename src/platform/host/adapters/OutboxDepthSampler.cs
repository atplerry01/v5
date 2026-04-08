using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Messaging;

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
    private static readonly ObservableGauge<long> DepthGauge =
        Meter.CreateObservableGauge("outbox.depth", () => Volatile.Read(ref _latestDepth));
    private static readonly ObservableGauge<double> OldestAgeGauge =
        Meter.CreateObservableGauge(
            "outbox.oldest_pending_age_seconds",
            () => Volatile.Read(ref _latestOldestAgeSeconds));

    private readonly string _connectionString;
    private readonly IOutboxDepthSnapshot _snapshot;
    private readonly TimeSpan _samplingInterval;
    private readonly ILogger<OutboxDepthSampler>? _logger;

    public OutboxDepthSampler(
        string connectionString,
        IOutboxDepthSnapshot snapshot,
        OutboxOptions options,
        ILogger<OutboxDepthSampler>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(options);
        if (options.SamplingIntervalSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.SamplingIntervalSeconds,
                "OutboxOptions.SamplingIntervalSeconds must be at least 1.");

        _connectionString = connectionString;
        _snapshot = snapshot;
        _samplingInterval = TimeSpan.FromSeconds(options.SamplingIntervalSeconds);
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
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT COUNT(*)::bigint,
                   COALESCE(EXTRACT(EPOCH FROM (NOW() - MIN(created_at))), 0)::double precision
              FROM outbox
             WHERE status IN ('pending','failed')
            """,
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return;

        var depth = reader.GetInt64(0);
        var oldestAgeSeconds = reader.GetDouble(1);

        Volatile.Write(ref _latestDepth, depth);
        Volatile.Write(ref _latestOldestAgeSeconds, oldestAgeSeconds);
        _snapshot.Publish(depth, oldestAgeSeconds);
    }
}
