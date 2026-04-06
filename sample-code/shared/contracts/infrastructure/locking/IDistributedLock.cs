namespace Whycespace.Shared.Contracts.Infrastructure.Locking;

/// <summary>
/// Infrastructure contract for distributed locking.
/// Implementations live in infrastructure/adapters — runtime acquires before engine execution.
/// Used by ChainWriteLockMiddleware to enforce single-writer semantics across nodes.
/// </summary>
public interface IDistributedLock
{
    Task<bool> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct = default);
    Task ReleaseAsync(string key, CancellationToken ct = default);
}
