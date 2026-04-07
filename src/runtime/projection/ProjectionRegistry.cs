using System.Collections.Concurrent;
using Whyce.Shared.Contracts.Projection;

namespace Whyce.Runtime.Projection;

/// <summary>
/// Projection Registry — maintains the mapping from event types to projection handlers.
/// Thread-safe for concurrent registration during bootstrap.
/// Locked after build to prevent runtime modification.
/// </summary>
public sealed class ProjectionRegistry
{
    private readonly ConcurrentDictionary<string, List<IEnvelopeProjectionHandler>> _handlers = new();
    private bool _locked;

    /// <summary>
    /// Registers a projection handler for a specific event type.
    /// Must be called during bootstrap, before the registry is locked.
    /// </summary>
    public void Register(string eventType, IEnvelopeProjectionHandler handler)
    {
        if (_locked)
            throw new InvalidOperationException("ProjectionRegistry is locked. Cannot register handlers after build.");

        ArgumentNullException.ThrowIfNull(handler);
        var handlers = _handlers.GetOrAdd(eventType, _ => new List<IEnvelopeProjectionHandler>());
        handlers.Add(handler);
    }

    /// <summary>
    /// Registers a projection handler for multiple event types.
    /// </summary>
    public void Register(IEnumerable<string> eventTypes, IEnvelopeProjectionHandler handler)
    {
        foreach (var eventType in eventTypes)
        {
            Register(eventType, handler);
        }
    }

    /// <summary>
    /// Resolves all handlers registered for a given event type.
    /// Returns empty collection if no handlers are registered.
    /// </summary>
    public IReadOnlyList<IEnvelopeProjectionHandler> ResolveHandlers(string eventType)
    {
        return _handlers.TryGetValue(eventType, out var handlers)
            ? handlers.AsReadOnly()
            : [];
    }

    /// <summary>
    /// Locks the registry. No further registrations are allowed.
    /// </summary>
    public void Lock()
    {
        _locked = true;
    }
}
