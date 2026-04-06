namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Resolves canonical Kafka topic names from EventEnvelope routing metadata.
/// SINGLE SOURCE OF TRUTH for topic resolution in the runtime layer.
///
/// Canonical format: whyce.{classification}.{context}.{domain}.{type}
/// where type ∈ {commands, events, deadletter, retry}
///
/// Enforces: lowercase, no double prefix, exactly 4 segments + whyce root.
/// </summary>
public sealed class TopicNameResolver
{
    private const string TopicPrefix = "whyce.";
    private static readonly HashSet<string> ValidChannelTypes = new(StringComparer.Ordinal)
    {
        "commands", "events", "deadletter", "retry"
    };

    /// <summary>
    /// Resolves the canonical Kafka topic for a given EventEnvelope and channel type.
    /// </summary>
    /// <param name="envelope">The event envelope containing routing metadata.</param>
    /// <param name="type">Channel type: "events", "commands", "deadletter", or "retry".</param>
    /// <returns>Canonical topic name (e.g., "whyce.operational.sandbox.todo.events").</returns>
    public string Resolve(EventEnvelope envelope, string type)
    {
        if (string.IsNullOrWhiteSpace(envelope.Classification))
            throw new InvalidTopicException(
                $"EventEnvelope.Classification is required for topic resolution. EventId={envelope.EventId}");

        if (string.IsNullOrWhiteSpace(envelope.Context))
            throw new InvalidTopicException(
                $"EventEnvelope.Context is required for topic resolution. EventId={envelope.EventId}");

        if (string.IsNullOrWhiteSpace(envelope.Domain))
            throw new InvalidTopicException(
                $"EventEnvelope.Domain is required for topic resolution. EventId={envelope.EventId}");

        var topic = $"whyce.{envelope.Classification}.{envelope.Context}.{envelope.Domain}.{type}";
        topic = Normalize(topic);
        Validate(topic, envelope.EventId);
        return topic;
    }

    private static string Normalize(string topic)
    {
        topic = topic.ToLowerInvariant();

        while (topic.StartsWith("whyce.whyce.", StringComparison.Ordinal))
        {
            topic = topic["whyce.".Length..];
        }

        if (!topic.StartsWith(TopicPrefix, StringComparison.Ordinal))
        {
            topic = $"{TopicPrefix}{topic}";
        }

        return topic;
    }

    private static void Validate(string topic, Guid eventId)
    {
        if (topic.Contains("whyce.whyce."))
        {
            throw new InvalidTopicException(
                $"Duplicate 'whyce.' prefix detected: {topic}. EventId={eventId}");
        }

        if (!topic.StartsWith(TopicPrefix, StringComparison.Ordinal))
        {
            throw new InvalidTopicException(
                $"Topic must start with 'whyce.': {topic}. EventId={eventId}");
        }

        if (topic != topic.ToLowerInvariant())
        {
            throw new InvalidTopicException(
                $"Topic must be lowercase: {topic}. EventId={eventId}");
        }

        var segments = topic.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 5)
        {
            throw new InvalidTopicException(
                $"Invalid topic segment count: {segments.Length}. " +
                $"Must be 5 (whyce.classification.context.domain.type). " +
                $"Topic: {topic}. EventId={eventId}");
        }

        if (segments[0] != "whyce")
        {
            throw new InvalidTopicException(
                $"Invalid root segment '{segments[0]}'. Expected 'whyce'. " +
                $"Topic: {topic}. EventId={eventId}");
        }

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
