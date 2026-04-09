using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Tests.Integration.Setup;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly HashSet<string> _keys = new();
    private readonly object _lock = new();

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock) return Task.FromResult(_keys.Contains(key));
    }

    public Task MarkAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock) _keys.Add(key);
        return Task.CompletedTask;
    }
}
