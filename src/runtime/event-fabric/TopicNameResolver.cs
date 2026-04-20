namespace Whycespace.Runtime.EventFabric;

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
    /// <summary>
    /// Resolves the canonical dead-letter topic name from a source topic
    /// string. The single source of truth for DLQ naming — callers MUST
    /// route through this method instead of inlining string manipulation.
    ///
    /// phase1.6-S1.6 (DLQ-RESOLVER-01): introduced to centralize the DLQ
    /// naming convention previously duplicated in
    /// <c>KafkaOutboxPublisher.TryPublishToDeadletterAsync</c>. Behavior:
    /// <list type="bullet">
    ///   <item>null / empty / whitespace topic → <see cref="ArgumentException"/></item>
    ///   <item>topic already ends in <c>.deadletter</c> → returned unchanged (idempotent)</item>
    ///   <item>topic ending in <c>.events</c> → suffix replaced with <c>.deadletter</c></item>
    ///   <item>any other suffix → <c>.deadletter</c> appended</item>
    /// </list>
    /// Idempotency is load-bearing: a recovery loop that re-publishes a
    /// row whose source topic was already a DLQ topic must not produce
    /// <c>x.deadletter.deadletter</c>. The unit tests pin every branch.
    /// </summary>
    public string ResolveDeadLetter(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException(
                "Topic must not be null, empty, or whitespace.", nameof(topic));

        if (topic.EndsWith(".deadletter", StringComparison.Ordinal))
            return topic;

        if (topic.EndsWith(".events", StringComparison.Ordinal))
            return string.Concat(
                topic.AsSpan(0, topic.Length - ".events".Length),
                ".deadletter");

        // R2.A.3a (R-TOPIC-TIER-01): retry → deadletter is a canonical
        // tier transition. A message that exhausts retry attempts flows
        // from `.retry` to `.deadletter`; callers MUST go through this
        // method rather than splice strings inline.
        if (topic.EndsWith(".retry", StringComparison.Ordinal))
            return string.Concat(
                topic.AsSpan(0, topic.Length - ".retry".Length),
                ".deadletter");

        return topic + ".deadletter";
    }

    /// <summary>
    /// R2.A.3a (R-TOPIC-TIER-01) — resolves the canonical retry topic name
    /// from a primary `.events` topic. The first tier in the D2 locked
    /// three-tier shape: `.events` → `.retry` → `.deadletter`.
    ///
    /// Behavior:
    /// <list type="bullet">
    ///   <item>null / empty / whitespace topic → <see cref="ArgumentException"/></item>
    ///   <item>topic already ends in <c>.retry</c> → returned unchanged (idempotent)</item>
    ///   <item>topic ending in <c>.events</c> → suffix replaced with <c>.retry</c></item>
    ///   <item>topic ending in <c>.deadletter</c> → <see cref="InvalidTopicException"/>
    ///         (moving backwards from terminal tier is forbidden)</item>
    ///   <item>topic ending in <c>.commands</c> → <see cref="InvalidTopicException"/>
    ///         (command-tier retry is not the outbox retry tier; use
    ///         <c>IRetryExecutor</c> at the command dispatch site instead)</item>
    ///   <item>any other suffix → <c>.retry</c> appended</item>
    /// </list>
    /// Idempotency on the `.retry` suffix mirrors <see cref="ResolveDeadLetter"/>
    /// so a re-drive of an already-retry-tier message does not produce
    /// <c>x.retry.retry</c>.
    /// </summary>
    public string ResolveRetry(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException(
                "Topic must not be null, empty, or whitespace.", nameof(topic));

        if (topic.EndsWith(".retry", StringComparison.Ordinal))
            return topic;

        if (topic.EndsWith(".deadletter", StringComparison.Ordinal))
            throw new InvalidTopicException(
                $"Cannot resolve retry tier from deadletter topic '{topic}'. " +
                "Deadletter is terminal; re-drive requires an explicit operator control.");

        if (topic.EndsWith(".commands", StringComparison.Ordinal))
            throw new InvalidTopicException(
                $"Cannot resolve retry tier from command topic '{topic}'. " +
                "Command-dispatch retry belongs to IRetryExecutor at the call site, " +
                "not to the outbox fabric tier.");

        if (topic.EndsWith(".events", StringComparison.Ordinal))
            return string.Concat(
                topic.AsSpan(0, topic.Length - ".events".Length),
                ".retry");

        return topic + ".retry";
    }

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
