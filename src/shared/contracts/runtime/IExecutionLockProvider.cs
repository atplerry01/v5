namespace Whyce.Shared.Contracts.Runtime;

/// <summary>
/// phase1.5-S5.2.5 / MI-1 (DISTRIBUTED-EXECUTION-SAFETY-01):
/// distributed mutual-exclusion seam used by
/// <c>RuntimeControlPlane</c> to guarantee that the same
/// <c>CommandId</c> cannot be executed concurrently across multiple
/// host instances. The contract lives in shared/contracts so the
/// runtime can reference it without taking a host or infrastructure
/// dependency (DG-R5-EXCEPT-01: runtime → host is forbidden).
///
/// Semantics:
///   - <see cref="TryAcquireAsync"/> attempts a non-blocking
///     acquire. Returns <c>true</c> if this caller now owns the
///     lock; <c>false</c> if another caller currently holds it.
///     Implementations MUST be safe under concurrent calls and
///     MUST NOT allow two callers to simultaneously observe
///     <c>true</c> for the same key.
///   - <see cref="ReleaseAsync"/> releases the lock owned by THIS
///     caller. Implementations MUST be owner-safe — releasing a
///     key not owned by this caller MUST be a no-op so a stale
///     process cannot accidentally unlock a key another owner
///     re-acquired.
///
/// Lease ceiling is enforced by the <c>ttl</c> argument to
/// <see cref="TryAcquireAsync"/>. MI-1 does NOT introduce lease
/// renewal — a request exceeding the lease implicitly loses
/// ownership at the underlying store. Renewal is reserved for a
/// future workstream.
/// </summary>
public interface IExecutionLockProvider
{
    Task<bool> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct);
    Task ReleaseAsync(string key);
}
