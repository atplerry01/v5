using System.Collections.Concurrent;

namespace Whycespace.Runtime.Routing;

/// <summary>
/// Resolves domain routes from command types and engine command types.
/// All routing flows through this resolver — no hardcoded domain paths.
///
/// Registration pattern:
///   resolver.Register(route, "create", "assign", "escalate", ...);
///
/// Resolution:
///   var route = resolver.Resolve("operational.incident.create");
/// </summary>
public sealed class DomainRouteResolver
{
    private readonly ConcurrentDictionary<string, DomainRoute> _commandTypeToRoute = new();
    private readonly ConcurrentDictionary<string, DomainRoute> _qualifiedPathToRoute = new();
    private volatile bool _isLocked;

    /// <summary>
    /// Registers a domain route with its supported actions.
    /// Each action becomes a resolvable command type: route.QualifiedPath + "." + action.
    /// </summary>
    public void Register(DomainRoute route, params string[] actions)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (_isLocked)
            throw new InvalidOperationException(
                "DomainRouteResolver is locked. Registration is only allowed during startup.");

        _qualifiedPathToRoute[route.QualifiedPath] = route;

        foreach (var action in actions)
        {
            var commandType = route.ResolveCommandType(action);
            if (!_commandTypeToRoute.TryAdd(commandType, route))
                throw new InvalidOperationException(
                    $"Command type '{commandType}' is already registered.");
        }
    }

    /// <summary>
    /// Resolves a domain route from a fully-qualified command type.
    /// </summary>
    public DomainRoute? Resolve(string commandType)
    {
        return _commandTypeToRoute.TryGetValue(commandType, out var route) ? route : null;
    }

    /// <summary>
    /// Resolves a domain route from its qualified path.
    /// </summary>
    public DomainRoute? ResolveByPath(string qualifiedPath)
    {
        return _qualifiedPathToRoute.TryGetValue(qualifiedPath, out var route) ? route : null;
    }

    /// <summary>
    /// Extracts the action from a command type using the registered route.
    /// e.g. "operational.incident.create" → "create"
    /// </summary>
    public string? ExtractAction(string commandType)
    {
        if (!_commandTypeToRoute.TryGetValue(commandType, out var route))
            return null;

        var prefix = route.QualifiedPath + ".";
        return commandType.StartsWith(prefix, StringComparison.Ordinal)
            ? commandType[prefix.Length..]
            : null;
    }

    /// <summary>
    /// Extracts the aggregate type from a command type using the registered route.
    /// Returns the route's QualifiedPath (domain-driven, not string-split).
    /// </summary>
    public string ResolveAggregateType(string commandType)
    {
        return _commandTypeToRoute.TryGetValue(commandType, out var route)
            ? route.AggregateType
            : FallbackAggregateType(commandType);
    }

    /// <summary>
    /// Resolves the projection name for a command type's domain context.
    /// </summary>
    public string? ResolveProjectionName(string commandType)
    {
        return _commandTypeToRoute.TryGetValue(commandType, out var route)
            ? route.ProjectionName
            : null;
    }

    public void Lock() => _isLocked = true;
    public bool IsLocked => _isLocked;
    public int RouteCount => _qualifiedPathToRoute.Count;
    public int CommandTypeCount => _commandTypeToRoute.Count;

    public IReadOnlyList<DomainRoute> GetAllRoutes() => [.. _qualifiedPathToRoute.Values];

    /// <summary>
    /// Fallback for unregistered command types: drops the last segment.
    /// e.g. "legacy.ping.execute" → "legacy.ping"
    /// </summary>
    private static string FallbackAggregateType(string commandType)
    {
        var lastDot = commandType.LastIndexOf('.');
        return lastDot > 0 ? commandType[..lastDot] : commandType;
    }
}
