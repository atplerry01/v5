using System.Collections.Concurrent;
using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Test-only in-memory <see cref="ISequenceStore"/>. Mirrors the production
/// Postgres adapter's atomic UPSERT semantics via
/// <see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate(TKey, TValue, System.Func{TKey, TValue, TValue})"/>.
/// FORBIDDEN in production wiring (deterministic-id.guard.md G16/A13).
/// </summary>
public sealed class InMemorySequenceStore : ISequenceStore
{
    private readonly ConcurrentDictionary<string, long> _counters = new();

    public Task<long> NextAsync(string scope)
    {
        var next = _counters.AddOrUpdate(scope, 1, (_, current) => current + 1);
        return Task.FromResult(next);
    }

    public Task<bool> HealthCheckAsync() => Task.FromResult(true);
}
