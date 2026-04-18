using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Phase 8 B7 — configurable <see cref="IAggregateStateLoader"/> for
/// policy-input-enrichment tests. Lets a test register per-command-type
/// snapshots and then verifies the middleware threaded them onto
/// <c>PolicyContext.ResourceState</c> verbatim.
///
/// <para>
/// <b>Null semantics.</b> When no snapshot is registered for the supplied
/// <c>commandType</c>, the loader returns <c>null</c> — matching the
/// production <c>NullAggregateStateLoader</c> default and the canonical
/// B6 "factory command" contract (aggregate does not exist yet).
/// </para>
/// </summary>
public sealed class StubAggregateStateLoader : IAggregateStateLoader
{
    private readonly Dictionary<Type, object?> _snapshots = new();
    private readonly List<(Type CommandType, Guid AggregateId)> _calls = new();

    public IReadOnlyList<(Type CommandType, Guid AggregateId)> Calls => _calls;

    /// <summary>
    /// Register the snapshot to return for a given command type. Pass
    /// <c>null</c> to simulate "aggregate does not exist yet" (equivalent
    /// to not registering, but explicit for test readability).
    /// </summary>
    public void Register<TCommand>(object? snapshot) =>
        _snapshots[typeof(TCommand)] = snapshot;

    public Task<object?> LoadSnapshotAsync(
        Type commandType,
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        _calls.Add((commandType, aggregateId));
        return Task.FromResult(_snapshots.TryGetValue(commandType, out var snap) ? snap : null);
    }
}
