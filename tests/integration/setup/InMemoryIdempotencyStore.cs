using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// In-memory <see cref="IIdempotencyStore"/> for integration tests.
///
/// phase1.5-S5.2.5 / TB-1: aligned with the post-KC-2 interface that adds
/// <c>TryClaimAsync</c> + <c>ReleaseAsync</c> as the canonical hot-path
/// claim shape, while keeping the obsolete two-step
/// <c>ExistsAsync</c> + <c>MarkAsync</c> for source-compatibility.
/// </summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly HashSet<string> _keys = new();
    private readonly object _lock = new();

    public Task<bool> TryClaimAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // First-seen → true, duplicate → false. Mirrors the production
            // semantics of PostgresIdempotencyStoreAdapter.TryClaimAsync.
            return Task.FromResult(_keys.Add(key));
        }
    }

    public Task ReleaseAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock) _keys.Remove(key);
        return Task.CompletedTask;
    }

#pragma warning disable CS0618 // obsolete two-step API retained for source-compat per IIdempotencyStore
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock) return Task.FromResult(_keys.Contains(key));
    }

    public Task MarkAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock) _keys.Add(key);
        return Task.CompletedTask;
    }
#pragma warning restore CS0618
}
