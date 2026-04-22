using Microsoft.Extensions.Hosting;
using Whycespace.Runtime.Security;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Phase 8 B5 — timer-driven BackgroundService that scans the lock
/// projection for naturally-expired rows and dispatches
/// ExpireSystemLockCommand for each via
/// <see cref="SystemLockExpirySchedulerHandler"/>. Shape parity with
/// <see cref="SanctionExpirySchedulerWorker"/>; only the query, handler,
/// and worker-name change.
///
/// <para>
/// <b>Suspend safety.</b> The query filters <c>status = 'Locked'</c>, so
/// a suspended lock is outside the candidate stream until resumed — the
/// scheduler cannot race the compensation-suspend flow into an expiry
/// for a paused timer.
/// </para>
/// </summary>
public sealed class SystemLockExpirySchedulerWorker : BackgroundService
{
    public const string WorkerName = "system-lock-expiry-scheduler";
    private const string SystemActor = "system/system-lock-expiry-scheduler";

    private readonly IExpirableLockQuery _query;
    private readonly SystemLockExpirySchedulerHandler _handler;
    private readonly IClock _clock;
    private readonly IWorkerLivenessRegistry _liveness;
    private readonly TimeSpan _interval;
    private readonly int _batchSize;
    private readonly ILogger<SystemLockExpirySchedulerWorker>? _logger;

    // R2.A.C.2 / R-LEADER-ELECTION-01 — optional distributed lease. When
    // registered (host composition via R2.A.C.1 PostgresAdvisoryLeaseProvider),
    // exactly one instance across N replicas runs the scan loop; others idle
    // at the lease gate. When null (legacy test hosts), every instance scans
    // independently — correct via idempotency middleware but wasteful.
    // Per-worker audit verdict in claude/new-rules/20260419-100000-guards.md.
    private readonly IDistributedLeaseProvider? _leaseProvider;
    private static readonly TimeSpan LeaderRetryInterval = TimeSpan.FromSeconds(30);

    public SystemLockExpirySchedulerWorker(
        IExpirableLockQuery query,
        SystemLockExpirySchedulerHandler handler,
        IClock clock,
        IWorkerLivenessRegistry liveness,
        int intervalSeconds,
        int batchSize,
        ILogger<SystemLockExpirySchedulerWorker>? logger = null)
        : this(query, handler, clock, liveness, intervalSeconds, batchSize, logger, leaseProvider: null)
    {
    }

    public SystemLockExpirySchedulerWorker(
        IExpirableLockQuery query,
        SystemLockExpirySchedulerHandler handler,
        IClock clock,
        IWorkerLivenessRegistry liveness,
        int intervalSeconds,
        int batchSize,
        ILogger<SystemLockExpirySchedulerWorker>? logger,
        IDistributedLeaseProvider? leaseProvider)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(liveness);
        if (intervalSeconds < 1)
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), intervalSeconds,
                "SystemLockExpirySchedulerWorker interval must be at least 1 second.");
        if (batchSize < 1)
            throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize,
                "SystemLockExpirySchedulerWorker batch size must be at least 1.");

        _query = query;
        _handler = handler;
        _clock = clock;
        _liveness = liveness;
        _interval = TimeSpan.FromSeconds(intervalSeconds);
        _batchSize = batchSize;
        _logger = logger;
        _leaseProvider = leaseProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation(
            "SystemLockExpirySchedulerWorker started: interval={IntervalSeconds}s batch={BatchSize} leaderMode={LeaderMode}.",
            (int)_interval.TotalSeconds, _batchSize, _leaseProvider is null ? "off" : "on");

        if (_leaseProvider is null)
        {
            await ScanLoopAsync(stoppingToken);
        }
        else
        {
            var holder = $"{Environment.MachineName}:{Environment.ProcessId}";
            await LeaderElection.RunAsLeaderAsync(
                _leaseProvider,
                leaseKey: WorkerName,
                holder: holder,
                leaderWork: ScanLoopAsync,
                retryInterval: LeaderRetryInterval,
                logger: _logger,
                cancellationToken: stoppingToken);
        }

        _logger?.LogInformation("SystemLockExpirySchedulerWorker stopping.");
    }

    private async Task ScanLoopAsync(CancellationToken stoppingToken)
    {
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
                    "SystemLockExpirySchedulerWorker iteration failed; liveness not stamped this tick.");
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
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
                    "SystemLockExpirySchedulerWorker failed to dispatch expiry for LockId={LockId} ExpiresAt={ExpiresAt:O}; claim released, next tick will retry.",
                    candidate.LockId, candidate.ExpiresAt);
            }
        }
    }
}
