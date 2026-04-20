using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Runtime.Resilience;

/// <summary>
/// R2.A.C.2 / R-LEADER-ELECTION-01 — composable helper for BackgroundService
/// workers that must run at most one instance across N replicas. Wraps
/// <see cref="IDistributedLeaseProvider"/> acquire + dispose around the
/// worker's existing loop body.
///
/// <para>
/// <b>NOT a replacement for row-lock / idempotency-based multi-instance
/// safety.</b> Workers whose current design documents row-level ownership
/// (e.g. <c>KafkaOutboxPublisher</c>'s MI-2 <c>FOR UPDATE SKIP LOCKED</c>
/// pattern) MUST NOT be migrated to this helper — see R-MULTI-INSTANCE-AUDIT-01.
/// Use this helper for workers that duplicate effort across instances
/// (periodic scan-and-dispatch, metric samplers that emit to external
/// stores that can't dedup).
/// </para>
///
/// <para>
/// <b>Leader-death-to-replacement gap</b> is bounded by Postgres TCP
/// keepalive window (~30-120s) plus <paramref name="retryInterval"/>. Workers
/// that cannot tolerate this gap must pick a different pattern.
/// </para>
/// </summary>
public static class LeaderElection
{
    /// <summary>
    /// Run <paramref name="leaderWork"/> inside a loop that holds a distributed
    /// lease. On contention (another instance is leader), waits
    /// <paramref name="retryInterval"/> and retries. On leader-side exception,
    /// releases the lease and retries after the same interval. Exits cleanly
    /// on cancellation.
    /// </summary>
    /// <param name="leaseProvider">Underlying lease primitive.</param>
    /// <param name="leaseKey">
    /// Worker-role identifier (NOT per-instance). Multiple instances of the
    /// same worker pass the same key; the one that acquires runs as leader.
    /// </param>
    /// <param name="holder">
    /// Short identifier of THIS instance — typically
    /// <c>$"{Environment.MachineName}:{Environment.ProcessId}"</c>. Observability only.
    /// </param>
    /// <param name="leaderWork">
    /// The work loop body. Runs under the lease until it returns or throws.
    /// Typically a <c>while (!ct.IsCancellationRequested) { ... }</c> loop.
    /// </param>
    /// <param name="retryInterval">
    /// Wait between acquire attempts when another instance holds the lease.
    /// Typical: 30s for expiry-class workers.
    /// </param>
    public static async Task RunAsLeaderAsync(
        IDistributedLeaseProvider leaseProvider,
        string leaseKey,
        string holder,
        Func<CancellationToken, Task> leaderWork,
        TimeSpan retryInterval,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leaseProvider);
        ArgumentNullException.ThrowIfNull(leaderWork);
        if (string.IsNullOrWhiteSpace(leaseKey))
            throw new ArgumentException("leaseKey is required.", nameof(leaseKey));
        if (string.IsNullOrWhiteSpace(holder))
            throw new ArgumentException("holder is required.", nameof(holder));
        if (retryInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(retryInterval),
                retryInterval, "retryInterval must be positive.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var lease = await leaseProvider.TryAcquireAsync(leaseKey, holder, cancellationToken);
                if (lease is null)
                {
                    logger?.LogDebug(
                        "LeaderElection: lease {Key} held by another instance; retrying in {Interval}.",
                        leaseKey, retryInterval);

                    try { await Task.Delay(retryInterval, cancellationToken); }
                    catch (OperationCanceledException) { return; }
                    continue;
                }

                await using (lease)
                {
                    logger?.LogInformation(
                        "LeaderElection: acquired lease {Key} as {Holder} at {AcquiredAt:O}.",
                        leaseKey, holder, lease.AcquiredAt);

                    await leaderWork(cancellationToken);
                }

                // leaderWork returned normally (cancellation or clean exit).
                // The await using above has released the lease. Loop continues
                // which re-checks cancellation and re-acquires if still needed.
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "LeaderElection: error under lease {Key}; released and retrying in {Interval}.",
                    leaseKey, retryInterval);

                try { await Task.Delay(retryInterval, cancellationToken); }
                catch (OperationCanceledException) { return; }
            }
        }
    }
}
