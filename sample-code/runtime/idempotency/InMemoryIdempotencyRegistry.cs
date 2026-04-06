using System.Collections.Concurrent;
using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Idempotency;

public sealed class InMemoryIdempotencyRegistry : IIdempotencyRegistry
{
    private readonly ConcurrentDictionary<Guid, CommandResult> _store = new();
    private readonly ConcurrentDictionary<string, CommandResult> _keyStore = new();

    public Task<bool> ExistsAsync(Guid commandId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_store.ContainsKey(commandId));
    }

    public Task<CommandResult?> GetResultAsync(Guid commandId, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(commandId, out var result);
        return Task.FromResult(result);
    }

    public Task RegisterAsync(Guid commandId, CommandResult result, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(commandId, result);
        return Task.CompletedTask;
    }

    public Task<CommandResult?> GetResultByKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        _keyStore.TryGetValue(idempotencyKey, out var result);
        return Task.FromResult(result);
    }

    public Task RegisterByKeyAsync(string idempotencyKey, CommandResult result, CancellationToken cancellationToken = default)
    {
        _keyStore.TryAdd(idempotencyKey, result);
        return Task.CompletedTask;
    }
}
