namespace Whycespace.Runtime.EventFabric.Topic;

/// <summary>
/// Resolves canonical Kafka topic names for runtime events.
/// SINGLE SOURCE OF TRUTH for all topic name generation.
///
/// Resolution priority:
///   1. Domain routing metadata (Cluster/SubCluster/Context) — preferred
///   2. EventType field — fallback, normalized to prevent double-prefix
///
/// Enforces WBSM v3 canonical topic law (type-based channels):
///   MULTI-CONTEXT: whyce.{classification}.{context}.{domain}.{type} → 5 segments
///   where type ∈ {commands, events, deadletter, retry}
///
/// Rejects ALL non-conforming topics.
/// </summary>
public sealed class TopicNameResolver : ITopicNameResolver
{
    private const string TopicPrefix = "whyce.";
    private static readonly HashSet<string> ValidChannelTypes = new(StringComparer.Ordinal)
    {
        "commands", "events", "deadletter", "retry"
    };

    public string Resolve(RuntimeEvent @event)
    {
        var topic = ResolveRawTopic(@event);
        topic = Normalize(topic);
        Validate(topic, @event.EventId);
        return topic;
    }

    /// <summary>
    /// Resolves the channel type suffix for a given message kind.
    /// Commands → ".commands", Events → ".events"
    /// </summary>
    public static string ResolveChannelType(bool isCommand) =>
        isCommand ? "commands" : "events";

    private static string ResolveRawTopic(RuntimeEvent @event)
    {
        // Priority 1: Use domain routing metadata if available
        if (@event.Cluster is not null && @event.Context is not null)
        {
            return BuildFromMetadata(@event);
        }

        // Priority 2: Use EventType with .events suffix (legacy/fallback path)
        return BuildFallbackTopic(@event.EventType);
    }

    /// <summary>
    /// Builds topic from event domain routing metadata.
    ///
    /// 5-segment (MULTI-CONTEXT):  whyce.{Cluster}.{SubCluster}.{Context}.events
    /// 4-segment (STANDARD):       whyce.{Cluster}.{Context}.events
    ///
    /// All domain events route to the `.events` channel for their BC.
    /// </summary>
    private static string BuildFromMetadata(RuntimeEvent @event)
    {
        if (!string.IsNullOrEmpty(@event.SubCluster))
        {
            // 5-segment MULTI-CONTEXT: whyce.{cluster}.{subcluster}.{context}.events
            return $"whyce.{@event.Cluster}.{@event.SubCluster}.{@event.Context}.events";
        }

        // 4-segment STANDARD: whyce.{cluster}.{context}.events
        return $"whyce.{@event.Cluster}.{@event.Context}.events";
    }

    /// <summary>
    /// Builds a fallback topic from EventType by stripping the event-specific suffix
    /// and appending ".events" channel type.
    /// e.g. "economic.ledger.transaction.initiated" → "whyce.economic.ledger.transaction.events"
    /// </summary>
    private static string BuildFallbackTopic(string eventType)
    {
        // Strip event-specific suffix and append channel type
        var lastDot = eventType.LastIndexOf('.');
        var basePath = lastDot >= 0 ? eventType[..lastDot] : eventType;
        return $"whyce.{basePath}.events";
    }

    private static string Normalize(string topic)
    {
        // Lowercase — canonical topics are always lowercase
        topic = topic.ToLowerInvariant();

        // Strip duplicate "whyce." prefix (prevents double-prefix from legacy EventType values)
        while (topic.StartsWith("whyce.whyce.", StringComparison.Ordinal))
        {
            topic = topic["whyce.".Length..];
        }

        // Ensure single "whyce." prefix
        if (!topic.StartsWith(TopicPrefix, StringComparison.Ordinal))
        {
            topic = $"{TopicPrefix}{topic}";
        }

        return topic;
    }

    private static void Validate(string topic, Guid eventId)
    {
        // 1. Double-prefix guard (post-normalization safety net)
        if (topic.Contains("whyce.whyce."))
        {
            throw new InvalidTopicException(
                $"Duplicate 'whyce.' prefix detected: {topic}. EventId={eventId}");
        }

        // 2. Root segment
        if (!topic.StartsWith(TopicPrefix, StringComparison.Ordinal))
        {
            throw new InvalidTopicException(
                $"Topic must start with 'whyce.': {topic}. EventId={eventId}");
        }

        // 3. Lowercase enforcement
        if (topic != topic.ToLowerInvariant())
        {
            throw new InvalidTopicException(
                $"Topic must be lowercase: {topic}. EventId={eventId}");
        }

        // 4. Segment count — STRICT: only 4 (STANDARD) or 5 (MULTI-CONTEXT)
        var segments = topic.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 4 && segments.Length != 5)
        {
            throw new InvalidTopicException(
                $"Invalid topic segment count: {segments.Length}. " +
                $"Must be 4 (STANDARD: whyce.classification.domain.type) " +
                $"or 5 (MULTI-CONTEXT: whyce.classification.context.domain.type). " +
                $"Topic: {topic}. EventId={eventId}");
        }

        // 5. Root segment value
        if (segments[0] != "whyce")
        {
            throw new InvalidTopicException(
                $"Invalid root segment '{segments[0]}'. Expected 'whyce'. " +
                $"Topic: {topic}. EventId={eventId}");
        }

        // 6. Channel type segment must be one of: commands, events, deadletter, retry
        var channelType = segments[^1];
        if (!ValidChannelTypes.Contains(channelType))
        {
            throw new InvalidTopicException(
                $"Invalid channel type '{channelType}'. " +
                $"Must be one of: commands, events, deadletter, retry. " +
                $"Topic: {topic}. EventId={eventId}");
        }
    }
}

public sealed class InvalidTopicException : Exception
{
    public InvalidTopicException(string message) : base(message) { }
}
