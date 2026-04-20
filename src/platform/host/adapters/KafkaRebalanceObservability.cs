using System.Diagnostics.Metrics;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.E.1 / R-CONSUMER-REBALANCE-OBSERVABILITY-HELPER-01 — canonical
/// wiring for Kafka consumer rebalance events. Every consumer worker
/// MUST <see cref="Attach{TKey,TValue}"/> this helper to its
/// <see cref="ConsumerBuilder{TKey,TValue}"/> before <c>.Build()</c>.
///
/// Three handlers are registered:
/// <list type="bullet">
///   <item><c>SetPartitionsAssignedHandler</c> — emits
///         <c>consumer.rebalance.assigned</c> counter + structured log.</item>
///   <item><c>SetPartitionsRevokedHandler</c> — emits
///         <c>consumer.rebalance.revoked</c> counter + structured log.
///         Our per-message <c>consumer.Commit(result)</c> pattern means
///         there is NO cross-message batch buffer that needs flushing
///         here — the revoke is safe by construction.</item>
///   <item><c>SetPartitionsLostHandler</c> — emits
///         <c>consumer.rebalance.lost</c> counter + structured log.
///         "Lost" is distinct from "revoked": it indicates a session
///         timeout / heartbeat failure and is a red-flag signal that
///         `SessionTimeoutMs` / `MaxPollIntervalMs` may be mis-sized or
///         the consumer loop was blocked too long.</item>
/// </list>
///
/// All observability flows to a dedicated <c>Whycespace.Kafka.Consumer</c>
/// meter so rebalance signals are observable across ALL consumer
/// workers — not just projection consumers — without coupling to any
/// specific worker's Meter.
///
/// Tagging is intentionally low cardinality: only <c>topic</c> and
/// <c>worker</c>. Per-partition tags are NOT emitted because the
/// partition identity is already carried by the structured log line
/// and exploding cardinality on a rebalance metric defeats the purpose.
/// </summary>
public static class KafkaRebalanceObservability
{
    public static readonly Meter Meter = new("Whycespace.Kafka.Consumer", "1.0");

    private static readonly Counter<long> RebalanceAssigned =
        Meter.CreateCounter<long>("consumer.rebalance.assigned");
    private static readonly Counter<long> RebalanceRevoked =
        Meter.CreateCounter<long>("consumer.rebalance.revoked");
    private static readonly Counter<long> RebalanceLost =
        Meter.CreateCounter<long>("consumer.rebalance.lost");

    /// <summary>
    /// Attaches rebalance-event observability to the supplied builder.
    /// Returns the same builder for fluent chaining.
    /// </summary>
    /// <param name="builder">The consumer builder being configured.</param>
    /// <param name="topic">Canonical topic name (tag).</param>
    /// <param name="workerName">Canonical worker identity (tag) — MUST
    /// match the worker's own liveness-registry name so rebalance events
    /// cross-reference cleanly with HC-5 liveness signals.</param>
    /// <param name="logger">Structured logger. Null-tolerant — production
    /// consumers always have one; tests may omit it.</param>
    public static ConsumerBuilder<TKey, TValue> Attach<TKey, TValue>(
        ConsumerBuilder<TKey, TValue> builder,
        string topic,
        string workerName,
        ILogger? logger)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(workerName);

        var topicTag = new KeyValuePair<string, object?>("topic", topic);
        var workerTag = new KeyValuePair<string, object?>("worker", workerName);

        builder.SetPartitionsAssignedHandler((_, partitions) =>
        {
            RebalanceAssigned.Add(1, topicTag, workerTag);
            logger?.LogInformation(
                "Kafka rebalance: assigned {PartitionCount} partition(s) to {Worker} on {Topic}: [{Partitions}]",
                partitions.Count, workerName, topic,
                string.Join(",", partitions.Select(p => p.Partition.Value)));
            // Returning null keeps default assignment behaviour. We
            // intentionally do NOT mutate the assigned set here —
            // cooperative-sticky already produces the minimal delta and
            // any override would defeat that invariant.
            return null;
        });

        builder.SetPartitionsRevokedHandler((_, partitions) =>
        {
            RebalanceRevoked.Add(1, topicTag, workerTag);
            logger?.LogInformation(
                "Kafka rebalance: revoking {PartitionCount} partition(s) from {Worker} on {Topic}: [{Partitions}]",
                partitions.Count, workerName, topic,
                string.Join(",", partitions.Select(p => p.TopicPartition.Partition.Value)));
            // Per-message commit contract means no pending commits to
            // flush here. Returning null accepts the default revoke
            // behaviour.
            return null;
        });

        builder.SetPartitionsLostHandler((_, partitions) =>
        {
            RebalanceLost.Add(1, topicTag, workerTag);
            logger?.LogWarning(
                "Kafka rebalance: LOST {PartitionCount} partition(s) for {Worker} on {Topic} (session timeout or heartbeat failure): [{Partitions}]",
                partitions.Count, workerName, topic,
                string.Join(",", partitions.Select(p => p.TopicPartition.Partition.Value)));
            return null;
        });

        return builder;
    }
}
