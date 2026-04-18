using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Phase 8 B5 — timer-driven BackgroundService that scans the sanction
/// projection for naturally-expired rows and dispatches
/// ExpireSanctionCommand for each via
/// <see cref="SanctionExpirySchedulerHandler"/>. Mirrors the shape of
/// <see cref="OutboxDepthSampler"/> for the periodic-scan pattern and
/// <see cref="SanctionActivationEnforcementWorker"/> for the
/// SystemIdentityScope + failure-isolation contract.
///
/// <para>
/// <b>Determinism.</b> A single <see cref="IClock.UtcNow"/> read per
/// iteration seeds both the projection scan and the idempotency keys for
/// every candidate in that batch, so two replicas ticking at the same
/// second produce identical keys and the <see cref="IIdempotencyStore"/>
/// picks exactly one winner per (SanctionId, ExpiresAt) pair.
/// </para>
///
/// <para>
/// <b>Failure isolation.</b> A per-candidate dispatch failure is caught,
/// logged, and does NOT stop the iteration — the other candidates in the
/// batch still get their chance. A full-iteration exception (scan
/// failure, cancellation, etc.) is caught, logged, and the worker sleeps
/// to the next tick. The only exit path is
/// <paramref name="stoppingToken"/> cancellation, honouring host
/// shutdown. Failure-release of a candidate's idempotency claim lets the
/// next scan naturally re-attempt.
/// </para>
///
/// <para>
/// <b>Liveness.</b> After each successful tick (even if the batch was
/// empty) the worker stamps <see cref="IWorkerLivenessRegistry"/> so
/// <c>WorkersHealthCheck</c> can assert the scheduler is alive — HC-5
/// WORKER-LIVENESS-01. A tick with a scan-level exception does NOT
/// stamp liveness, so a persistently-broken projection will age the
/// scheduler out of Ready on the configured MaxSilenceSeconds.
/// </para>
/// </summary>
public sealed class SanctionExpirySchedulerWorker : BackgroundService
{
    public const string WorkerName = "sanction-expiry-scheduler";
    private const string SystemActor = "system/sanction-expiry-scheduler";

    private readonly IExpirableSanctionQuery _query;
    private readonly SanctionExpirySchedulerHandler _handler;
    private readonly IClock _clock;
    private readonly IWorkerLivenessRegistry _liveness;
    private readonly TimeSpan _interval;
    private readonly int _batchSize;
    private readonly ILogger<SanctionExpirySchedulerWorker>? _logger;

    public SanctionExpirySchedulerWorker(
        IExpirableSanctionQuery query,
        SanctionExpirySchedulerHandler handler,
        IClock clock,
        IWorkerLivenessRegistry liveness,
        int intervalSeconds,
        int batchSize,
        ILogger<SanctionExpirySchedulerWorker>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(liveness);
        if (intervalSeconds < 1)
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), intervalSeconds,
                "SanctionExpirySchedulerWorker interval must be at least 1 second.");
        if (batchSize < 1)
            throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize,
                "SanctionExpirySchedulerWorker batch size must be at least 1.");

        _query = query;
        _handler = handler;
        _clock = clock;
        _liveness = liveness;
        _interval = TimeSpan.FromSeconds(intervalSeconds);
        _batchSize = batchSize;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation(
            "SanctionExpirySchedulerWorker started: interval={IntervalSeconds}s batch={BatchSize}.",
            (int)_interval.TotalSeconds, _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanOnceAsync(stoppingToken);
                _liveness.RecordSuccess(WorkerName, _clock.UtcNow);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "SanctionExpirySchedulerWorker iteration failed; liveness not stamped this tick.");
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }

        _logger?.LogInformation("SanctionExpirySchedulerWorker stopping.");
    }

    private async Task ScanOnceAsync(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var candidates = await _query.QueryExpirableAsync(now, _batchSize, cancellationToken);
        if (candidates.Count == 0) return;

        foreach (var candidate in candidates)
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                using (SystemIdentityScope.Begin(SystemActor, "system", "system"))
                {
                    await _handler.HandleAsync(candidate, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "SanctionExpirySchedulerWorker failed to dispatch expiry for SanctionId={SanctionId} ExpiresAt={ExpiresAt:O}; claim released, next tick will retry.",
                    candidate.SanctionId, candidate.ExpiresAt);
            }
        }
    }
}
