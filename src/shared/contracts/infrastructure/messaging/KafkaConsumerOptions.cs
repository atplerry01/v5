namespace Whycespace.Shared.Contracts.Infrastructure.Messaging;

/// <summary>
/// Tunable behavior for the runtime Kafka projection consumer worker.
///
/// phase1.5-S5.2.1 / PC-6 (KAFKA-CONSUMER-CONFIG-01): externalises the
/// previously incidental Confluent.Kafka / librdkafka defaults that
/// govern in-process prefetch buffering, fetch sizing, and session/poll
/// liveness. Step B / P-B6 confirmed the pre-PC-6 <c>ConsumerConfig</c>
/// set only <c>BootstrapServers</c>, <c>GroupId</c>,
/// <c>AutoOffsetReset</c>, and <c>EnableAutoCommit</c>; every other
/// load-bearing parameter inherited the librdkafka default
/// (<c>queued.max.messages.kbytes ≈ 1 GiB</c>), creating a large silent
/// in-process prefetch buffer between the broker and a slow projection
/// writer. PC-6 closes that gap by binding all four parameters from
/// configuration via this record.
///
/// Follows the phase1.6-S1.5 OutboxOptions / phase1.5-S5.2.1 OpaOptions
/// / IntakeOptions / PostgresPoolOptions precedent — a plain record,
/// no <c>IOptions&lt;T&gt;</c> indirection. Defaults are conservative:
/// the prefetch ceiling is sized to a single bounded batch of in-flight
/// projection writes rather than the librdkafka default, but the
/// session/poll cadence matches the librdkafka defaults to avoid
/// accidentally tightening rebalance behavior in this pass.
///
/// This pass is about declared bounded buffering and session/poll
/// timing only. The worker's sequential consume → handle → commit
/// shape is preserved verbatim.
/// </summary>
public sealed record KafkaConsumerOptions
{
    /// <summary>
    /// Maximum amount of message data, in kilobytes, that the
    /// librdkafka client will buffer in-process across all subscribed
    /// topic-partitions for this consumer. Maps directly to
    /// <c>queued.max.messages.kbytes</c>. Must be at least 1.
    ///
    /// Default 16384 (16 MiB) — three orders of magnitude tighter than
    /// the librdkafka default and sized so a slow projection writer
    /// produces consumer lag visible at the broker (correct
    /// backpressure shape) instead of silent in-process accumulation.
    /// </summary>
    public int QueuedMaxMessagesKbytes { get; init; } = 16384;

    /// <summary>
    /// Maximum size, in bytes, of a single message the consumer will
    /// fetch. Maps to <c>fetch.message.max.bytes</c>. Must be at least
    /// 1024. Default 1048576 (1 MiB) — matches the librdkafka default,
    /// kept explicit to honor the no-incidental-defaults rule (R-10).
    /// </summary>
    public int FetchMessageMaxBytes { get; init; } = 1048576;

    /// <summary>
    /// Maximum interval, in milliseconds, between successive
    /// <c>Consume</c> calls before the broker considers the consumer
    /// failed and triggers a rebalance. Maps to
    /// <c>max.poll.interval.ms</c>. Must be at least 1000. Default
    /// 300000 (5 minutes) — matches the librdkafka default.
    /// </summary>
    public int MaxPollIntervalMs { get; init; } = 300000;

    /// <summary>
    /// Consumer group session timeout, in milliseconds. The broker
    /// will fence a consumer that fails to heartbeat within this
    /// window. Maps to <c>session.timeout.ms</c>. Must be at least
    /// 1000. Default 45000 (45 seconds) — matches the librdkafka
    /// default.
    /// </summary>
    public int SessionTimeoutMs { get; init; } = 45000;
}
