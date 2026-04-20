namespace Whycespace.Shared.Contracts.Infrastructure.Persistence;

/// <summary>
/// R2.A.C.1 (D3 LOCKED → Postgres advisory locks) — canonical distributed
/// lease primitive. Single acquire operation; contention returns
/// <c>null</c> (NOT an exception) so callers don't need try/catch around
/// the hot path.
///
/// <b>DIFFERENT from <see cref="Whycespace.Shared.Contracts.Runtime.IExecutionLockProvider"/></b>
/// (Redis-backed, short-lived per-command execution lock, TTL-bounded).
/// <c>IDistributedLeaseProvider</c> is for longer-lived coordination:
/// <list type="bullet">
///   <item>leadership for background workers (one outbox publisher per
///         consumer group across N instances);</item>
///   <item>per-workflow serialization (R3 suspend/resume);</item>
///   <item>single-writer protection for aggregates that require strict
///         serialization beyond optimistic concurrency.</item>
/// </list>
///
/// <b>Crash-safe by construction</b> (R-LEASE-CRASH-SAFE-01): the Postgres
/// implementation holds a session-level advisory lock which the server
/// releases when the connection closes (process crash, network partition,
/// host termination). No application-level TTL, no client clock, no
/// clock-skew failure mode. See <see cref="ILease"/> for handle contract.
/// </summary>
public interface IDistributedLeaseProvider
{
    /// <summary>
    /// Try to acquire the lease. Returns the handle on success or
    /// <c>null</c> if another holder currently has it. MUST NOT throw on
    /// contention — null is the canonical "busy" signal. Exceptions are
    /// reserved for infrastructure failure (connection lost, etc.) per
    /// <see cref="PolicyEvaluationUnavailableException"/>-style discipline.
    ///
    /// Dispose the returned <see cref="ILease"/> to release. A lease MUST
    /// NOT be shared across threads without external synchronization.
    /// </summary>
    /// <param name="leaseKey">
    /// Application-level key (e.g. <c>"outbox-publisher-leader"</c> or
    /// <c>"workflow:lifecycle:{workflowId}"</c>). Callers own namespacing.
    /// </param>
    /// <param name="holder">
    /// Short identifier of the acquiring process — recorded on the handle
    /// for audit/observability. Typically <c>$"{hostName}:{processId}"</c>.
    /// </param>
    Task<ILease?> TryAcquireAsync(
        string leaseKey,
        string holder,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Handle returned by <see cref="IDistributedLeaseProvider.TryAcquireAsync"/>.
/// Dispose to release the lease. Double-dispose is a no-op.
/// </summary>
public interface ILease : IAsyncDisposable
{
    string Key { get; }
    string Holder { get; }
    DateTimeOffset AcquiredAt { get; }
}
