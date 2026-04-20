using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.E.4 / R-TOPIC-VERIFIER-HELPER-01 — stateless helper that queries
/// broker metadata and returns the subset of <paramref name="expected"/>
/// topics that do not currently exist on the broker.
///
/// Canonical usage pattern:
/// <code>
///   var missing = await KafkaTopicVerifier.FindMissingTopicsAsync(
///       adminClient, KafkaCanonicalTopics.All,
///       TimeSpan.FromSeconds(10), logger, stoppingToken);
/// </code>
///
/// Topic creation is handled by
/// <c>infrastructure/event-fabric/kafka/create-topics.sh</c> (operator-run
/// during infra setup). This verifier is startup-time alignment —
/// surfacing broker/runtime drift loudly rather than letting a missing
/// topic degrade into a <c>UNKNOWN_TOPIC_OR_PARTITION</c> error at consume
/// time.
///
/// Stateless. Safe to call from any thread. Does not mutate broker state.
/// </summary>
public static class KafkaTopicVerifier
{
    /// <summary>
    /// Returns the subset of <paramref name="expected"/> that does not
    /// appear in the broker's current topic list.
    /// </summary>
    /// <param name="adminClient">
    /// Admin client configured against the target broker. Caller owns its
    /// lifecycle — this method does NOT dispose the client.
    /// </param>
    /// <param name="expected">Canonical list of expected topic names.</param>
    /// <param name="metadataTimeout">
    /// Timeout for the <c>GetMetadata</c> call. Should be long enough for a
    /// loaded broker to respond (default 10s at call site).
    /// </param>
    /// <param name="logger">Null-tolerant structured logger.</param>
    /// <param name="cancellationToken">
    /// Propagates into the async wrap around the synchronous
    /// <c>GetMetadata</c> call.
    /// </param>
    public static Task<IReadOnlyList<string>> FindMissingTopicsAsync(
        IAdminClient adminClient,
        IEnumerable<string> expected,
        TimeSpan metadataTimeout,
        ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(adminClient);
        ArgumentNullException.ThrowIfNull(expected);
        if (metadataTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(metadataTimeout), metadataTimeout,
                "metadataTimeout must be positive.");

        var expectedSet = new HashSet<string>(expected, StringComparer.Ordinal);
        if (expectedSet.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        // IAdminClient.GetMetadata is synchronous. Wrap in Task.Run so the
        // caller's CancellationToken can propagate via the continuation
        // even though the broker call itself is not interruptible once
        // issued.
        return Task.Run<IReadOnlyList<string>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var metadata = adminClient.GetMetadata(metadataTimeout);
            var existing = new HashSet<string>(
                metadata.Topics.Select(t => t.Topic),
                StringComparer.Ordinal);

            var missing = expectedSet
                .Where(t => !existing.Contains(t))
                .OrderBy(t => t, StringComparer.Ordinal)
                .ToList();

            logger?.LogDebug(
                "Topic alignment check: {ExistingCount} broker topics, " +
                "{ExpectedCount} expected, {MissingCount} missing.",
                metadata.Topics.Count, expectedSet.Count, missing.Count);

            return missing;
        }, cancellationToken);
    }
}
