using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

/// <summary>
/// Rebuilds identity projections from the event store.
/// CLEARS all existing read models before replaying events from zero.
/// Replay-safe: handlers are idempotent, rebuild produces identical state.
/// Projections are NOT source of truth — event store is authoritative.
/// </summary>
public sealed class IdentityProjectionRebuilder
{
    private readonly IProjectionEventReader _eventReader;
    private readonly IdentityReadStore _readStore;

    private static readonly Dictionary<string, Func<ProjectionEvent, IdentityReadStore, CancellationToken, Task>> Handlers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IdentityRegisteredEvent"] = IdentityRegisteredHandler.HandleAsync,
        ["IdentityActivatedEvent"] = IdentityActivatedHandler.HandleAsync,
        ["IdentitySuspendedEvent"] = IdentitySuspendedHandler.HandleAsync,
        ["IdentityDeactivatedEvent"] = IdentityDeactivatedHandler.HandleAsync,
        ["SessionStartedEvent"] = SessionStartedHandler.HandleAsync,
        ["SessionRevokedEvent"] = SessionRevokedHandler.HandleAsync,
        ["RoleCreatedEvent"] = RoleCreatedHandler.HandleAsync,
        ["DeviceRegisteredEvent"] = DeviceRegisteredHandler.HandleAsync,
        ["AccessProfileCreatedEvent"] = AccessProfileCreatedHandler.HandleAsync,
    };

    public IdentityProjectionRebuilder(IProjectionEventReader eventReader, IdentityReadStore readStore)
    {
        _eventReader = eventReader;
        _readStore = readStore;
    }

    public async Task RebuildAsync(CancellationToken ct = default)
    {
        // Step 1: Clear ALL existing projection data and reset metrics to zero
        await _readStore.ClearAllAsync(ct);

        // Step 2: Replay all events from the beginning, ordered by timestamp
        var events = await _eventReader.ReadAllAsync(after: null, cancellationToken: ct);

        foreach (var @event in events.OrderBy(e => e.Timestamp))
        {
            if (Handlers.TryGetValue(@event.EventType, out var handler))
            {
                await handler(@event, _readStore, ct);
            }
        }
    }
}
