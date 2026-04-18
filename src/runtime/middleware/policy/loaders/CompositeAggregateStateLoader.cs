using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Runtime.Middleware.Policy.Loaders;

/// <summary>
/// Phase 11 B3 — <see cref="IAggregateStateLoader"/> that fans out by
/// command type to per-aggregate loaders. Registered as the single
/// <c>IAggregateStateLoader</c> in composition; supersedes
/// <c>NullAggregateStateLoader</c> as the default.
///
/// <para>
/// <b>Dispatch.</b> The composite holds its registered loaders in
/// construction-order. For each <c>LoadSnapshotAsync</c> call it asks
/// each loader via its static <c>Handles(commandType)</c> gate; the first
/// match wins. Unhandled command types fall through to <c>null</c> —
/// preserving the pre-B3 backward-compat contract for every command that
/// doesn't yet have a per-aggregate loader.
/// </para>
///
/// <para>
/// <b>Determinism.</b> Loader order is fixed at construction time. The
/// composite never consults DI, a timer, or any other ambient source —
/// given the same constructor argument and the same call, the dispatch
/// decision is byte-identical.
/// </para>
///
/// <para>
/// <b>Type-based gate, not reflection.</b> Each per-aggregate loader
/// exposes a static <c>Handles(Type)</c> predicate. The composite holds
/// a table of <c>(predicate, loader)</c> pairs supplied at construction.
/// Adding a new per-aggregate loader is a one-line composition change,
/// not a reflection scan.
/// </para>
/// </summary>
public sealed class CompositeAggregateStateLoader : IAggregateStateLoader
{
    public readonly record struct Route(
        Func<Type, bool> Handles,
        IAggregateStateLoader Loader);

    private readonly IReadOnlyList<Route> _routes;

    public CompositeAggregateStateLoader(IEnumerable<Route> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        _routes = routes.ToArray();
    }

    public async Task<object?> LoadSnapshotAsync(
        Type commandType,
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandType);

        foreach (var route in _routes)
        {
            if (route.Handles(commandType))
                return await route.Loader.LoadSnapshotAsync(
                    commandType, aggregateId, cancellationToken);
        }

        return null;
    }
}
