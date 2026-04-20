using System.Diagnostics.Metrics;
using Confluent.Kafka;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.E.2 / R-CONSUMER-LAG-HELPER-01 — canonical per-message consumer
/// lag sampling. Every consumer worker MUST call
/// <see cref="Record{TKey,TValue}"/> after each successful
/// <c>IConsumer{TKey,TValue}.Consume</c> that returns a non-null
/// <see cref="ConsumeResult{TKey,TValue}"/>.
///
/// Lag is computed as <c>HighWatermark - ConsumedOffset - 1</c>, where
/// <c>HighWatermark</c> is the partition's last-known next-offset-to-be-produced.
/// The <c>-1</c> is canonical: when we consume offset N, the high-watermark
/// is at-least N+1, so a read of the freshest message reports zero lag.
///
/// The <see cref="IConsumer{TKey,TValue}.GetWatermarkOffsets"/> call is a
/// fast in-process read of the cached watermark from the last
/// FetchResponse — NOT a broker round-trip. Safe to call after every
/// Consume() without measurable overhead.
///
/// Observability surface lives on the shared
/// <c>Whycespace.Kafka.Consumer</c> meter — the same meter that carries
/// the R2.E.1 <c>consumer.rebalance.*</c> counters. Rebalance and lag
/// signals are thus observable in a single scrape / dashboard.
///
/// Tagging:
/// <list type="bullet">
///   <item><c>topic</c> — canonical topic identity (stable across
///         rebalances). For multi-topic workers, the first topic is
///         used to match the R2.E.1 convention.</item>
///   <item><c>worker</c> — canonical worker name (same value used for
///         the rebalance counters).</item>
///   <item><c>partition</c> — partition number. Acceptable cardinality
///         (typically 3-12 per topic); matches standard Kafka-exporter
///         vocabulary.</item>
/// </list>
///
/// Failure mode: if <see cref="IConsumer{TKey,TValue}.GetWatermarkOffsets"/>
/// throws (transient cache miss immediately after assignment, before
/// the first fetch response), we emit a <c>consumer.lag_unknown</c>
/// counter instead of the histogram. This preserves the operator's
/// ability to distinguish "zero lag" from "couldn't read lag" — a
/// sustained <c>lag_unknown</c> signal indicates a consumer health
/// problem distinct from a healthy zero-lag consumer.
/// </summary>
public static class KafkaLagObservability
{
    // Reuse the same Meter that KafkaRebalanceObservability registered.
    // Multiple Meter instances sharing the same name/version are
    // collapsed by listeners (OTel, Prometheus exporter,
    // dotnet-counters), so the two files emit to the same logical
    // meter without coupling their code.
    private static readonly Meter Meter =
        new("Whycespace.Kafka.Consumer", "1.0");

    private static readonly Histogram<long> LagMessages =
        Meter.CreateHistogram<long>("consumer.lag_messages", unit: "messages");

    private static readonly Counter<long> LagUnknown =
        Meter.CreateCounter<long>("consumer.lag_unknown");

    // R2.E.3 / R-CONSUMER-PARTITION-SKEW-01: per-partition throughput
    // counter. Incremented in lockstep with LagMessages so operator
    // dashboards can compute partition skew as max:mean ratio across
    // {partition} tag values at a fixed window. Divergence with lag
    // observations is impossible by construction — both signals
    // emit from the same Record method body.
    private static readonly Counter<long> MessagesProcessed =
        Meter.CreateCounter<long>("consumer.messages_processed");

    /// <summary>
    /// Records the per-partition lag delta for the supplied consume result.
    /// Safe to call from any thread that owns the consumer — the
    /// watermark read is thread-local-cached state.
    /// </summary>
    /// <param name="consumer">The consumer instance that produced the result.</param>
    /// <param name="result">A non-null, non-partition-EOF consume result.</param>
    /// <param name="workerName">Canonical worker name (tag); same value
    /// used for the R2.E.1 rebalance counters.</param>
    /// <param name="topicTag">Canonical topic identity (tag); for
    /// multi-topic workers pass the first/primary topic.</param>
    public static void Record<TKey, TValue>(
        IConsumer<TKey, TValue> consumer,
        ConsumeResult<TKey, TValue> result,
        string workerName,
        string topicTag)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(workerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicTag);

        // A partition-EOF marker has Offset = Watermarks.High and no
        // message — emitting lag against it would double-count. The
        // Confluent.Kafka client does not surface partition-EOF results
        // unless EnablePartitionEof=true (we do not set it), so in the
        // standard path we always have a real message. Guard anyway.
        if (result.IsPartitionEOF) return;

        var topicTagKv = new KeyValuePair<string, object?>("topic", topicTag);
        var workerTagKv = new KeyValuePair<string, object?>("worker", workerName);
        var partitionTagKv = new KeyValuePair<string, object?>(
            "partition", result.Partition.Value);

        try
        {
            var watermarks = consumer.GetWatermarkOffsets(result.TopicPartition);
            // GetWatermarkOffsets returns the cached watermark from the
            // last FetchResponse. High is the next-offset-to-be-produced.
            // When we consumed `result.Offset`, the partition has
            // consumed-plus-one messages up to High; lag is the count
            // of messages still unconsumed AFTER this one.
            var lag = watermarks.High.Value - result.Offset.Value - 1;
            if (lag < 0) lag = 0; // defensive — clamp stale watermark cache

            LagMessages.Record(lag, topicTagKv, workerTagKv, partitionTagKv);

            // R2.E.3 / R-CONSUMER-PARTITION-SKEW-01: bump throughput
            // counter in lockstep with the lag observation. Operator
            // dashboards compute skew as max:mean ratio across the
            // {partition} tag values at a fixed time window.
            MessagesProcessed.Add(1, topicTagKv, workerTagKv, partitionTagKv);
        }
        catch (KafkaException)
        {
            // Watermark cache not yet populated (e.g. immediately after
            // a partition assignment, before the first fetch response).
            // Emit the unknown-lag signal so operators can distinguish
            // "zero lag" from "couldn't read lag" — a sustained
            // consumer.lag_unknown count indicates the consumer has
            // not completed a fetch cycle, NOT that it is caught up.
            //
            // R2.E.3: the lag-unknown path does NOT increment
            // messages_processed — semantic parity: if we couldn't
            // observe the message's position, we also don't claim
            // throughput for it.
            LagUnknown.Add(1, topicTagKv, workerTagKv, partitionTagKv);
        }
    }
}
