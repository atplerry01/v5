namespace Whycespace.Runtime.Routing;

/// <summary>
/// Resolves Kafka topics for events using the canonical channel model:
///
/// whyce.{classification}.{context}.{domain}.{type}
/// where type ∈ {commands, events, deadletter, retry}
///
/// 1. Domain Topic (MANDATORY for all events):
///    whyce.{cluster}.{subcluster}.{context}.events
///    e.g. whyce.operational.global.incident.events
///
/// 2. Global Topic (ONLY for global contexts like incident):
///    whyce.{cluster}.{context_leaf}.events
///    e.g. whyce.operational.incident.events
///
/// Projections MUST consume domain topics only.
/// Global topics are for cross-domain subscribers (alerting, audit, etc.).
/// </summary>
public sealed class TopicResolver
{
    private static readonly HashSet<string> GlobalContexts = new(StringComparer.OrdinalIgnoreCase)
    {
        "incident"
    };

    private readonly DomainRouteResolver _routeResolver;

    public TopicResolver(DomainRouteResolver routeResolver)
    {
        _routeResolver = routeResolver;
    }

    /// <summary>
    /// Resolves all topics an event should be published to.
    /// Returns domain topic (always) + global topic (if applicable).
    /// All events route to the `.events` channel for their BC.
    /// </summary>
    public IReadOnlyList<string> ResolveTopics(string eventType, string? cluster, string? subCluster, string? app, string? context)
    {
        var topics = new List<string>(2);

        // 1. Domain topic (mandatory) — routes to .events channel
        var domainTopic = ResolveDomainTopic(eventType, cluster, subCluster, app, context);
        topics.Add(domainTopic);

        // 2. Global topic (for incident and other global contexts)
        var globalTopic = ResolveGlobalTopic(eventType, cluster, context);
        if (globalTopic is not null)
        {
            topics.Add(globalTopic);
        }

        return topics;
    }

    /// <summary>
    /// Resolves the domain topic for an event.
    /// Format: whyce.{cluster}.{subcluster}.{app}.{context}.events
    ///
    /// Falls back to whyce.{basePath}.events for events without domain metadata.
    /// </summary>
    public string ResolveDomainTopic(string eventType, string? cluster, string? subCluster, string? app, string? context)
    {
        if (cluster is not null && subCluster is not null && app is not null && context is not null)
        {
            return $"whyce.{cluster}.{subCluster}.{context}.events";
        }

        // Fallback: strip event-specific suffix and append .events
        var lastDot = eventType.LastIndexOf('.');
        var basePath = lastDot >= 0 ? eventType[..lastDot] : eventType;
        return $"whyce.{basePath}.events";
    }

    /// <summary>
    /// Resolves the global topic for an event, if applicable.
    /// Format: whyce.{cluster}.{context_leaf}.events
    ///
    /// Returns null if the context is not a global context.
    /// </summary>
    public string? ResolveGlobalTopic(string eventType, string? cluster, string? context)
    {
        if (cluster is null || context is null)
            return null;

        var contextLeaf = ExtractContextLeaf(context);

        if (!GlobalContexts.Contains(contextLeaf))
            return null;

        return $"whyce.{cluster}.{contextLeaf}.events";
    }

    /// <summary>
    /// Extracts the leaf context from a potentially nested context.
    /// e.g. "incident" -> "incident"
    /// </summary>
    private static string ExtractContextLeaf(string context)
    {
        var lastDot = context.LastIndexOf('.');
        return lastDot >= 0 ? context[(lastDot + 1)..] : context;
    }
}
