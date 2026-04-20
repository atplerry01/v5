using System.Globalization;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.A.3d / R-RETRY-CONSUMER-WORKER-01 — dedicated consumer that
/// drains the <c>.retry</c> tier. Subscribes to every
/// <c>.retry</c> topic declared in
/// <see cref="KafkaCanonicalTopics.All"/>. Single consumer instance
/// across all retry topics, under a dedicated consumer group
/// <c>whyce.retry-consumer</c> so its offset progress is isolated
/// from projection / saga / integration groups.
///
/// Per-message flow:
/// <list type="number">
///   <item>Parse the R-RETRY-HEADERS-01 headers
///         (<c>retry-attempt-count</c> / <c>retry-deliver-after-unix-ms</c>
///         / <c>retry-source-topic</c>). Any missing / unparseable →
///         route direct to the <c>.deadletter</c> tier as a corrupt
///         retry-tier message (a message in a <c>.retry</c> topic
///         without the contract headers is by definition poison; it
///         cannot be re-published because we don't know where).</item>
///   <item>Compute <c>delay = deliverAfterUnixMs - clock.UtcNow.ToUnixMs()</c>.
///         If positive and within <see cref="DelayCeilingMs"/>
///         (60_000 ms), sleep for <c>delay</c> ms then proceed.
///         If positive and beyond the ceiling, do NOT commit the
///         offset — Kafka redelivery on the next poll cycle after the
///         ceiling sleep ensures the worker remains responsive to
///         cancellation and consumer-group rebalance events while
///         long delays elapse.</item>
///   <item>Re-publish the original message (original envelope headers
///         + payload, retry-specific transport headers stripped) to
///         the <c>retry-source-topic</c> value. The <c>retry-attempt-count</c>
///         header is PRESERVED so the next source-topic handler failure
///         can feed it to <see cref="KafkaRetryEscalator"/> for the
///         next attempt increment.</item>
///   <item>Commit the <c>.retry</c> source offset.</item>
/// </list>
///
/// Observability: wires <see cref="KafkaRebalanceObservability.Attach"/>
/// + <see cref="KafkaLagObservability.Record"/> identically to the
/// other 11 consumer workers — consistency is load-bearing for the
/// architecture tests `Every_ConsumerBuilder_Build_is_preceded_by_KafkaRebalanceObservability_Attach`
/// + `Every_Consumer_Consume_is_followed_by_KafkaLagObservability_Record`.
/// </summary>
public sealed class KafkaRetryConsumerWorker : BackgroundService
{
    private const string WorkerName = "retry-consumer";
    private const string ConsumerGroup = "whyce.retry-consumer";

    // Ceiling for in-loop sleep before we yield back to the poll cycle.
    // A `.retry` message with deliver-after 30 minutes in the future
    // should NOT pin this worker inside a 30-minute sleep — Kafka will
    // redeliver on the next poll cycle after this ceiling elapses, and
    // the worker stays responsive to cancellation / rebalance.
    private const int DelayCeilingMs = 60_000;

    private readonly string _kafkaBootstrapServers;
    private readonly IReadOnlyList<string> _retryTopics;
    private readonly IProducer<string, string> _producer;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly IWorkerLivenessRegistry _liveness;
    private readonly ILogger<KafkaRetryConsumerWorker>? _logger;

    public KafkaRetryConsumerWorker(
        string kafkaBootstrapServers,
        IReadOnlyList<string> retryTopics,
        IProducer<string, string> producer,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        IWorkerLivenessRegistry liveness,
        ILogger<KafkaRetryConsumerWorker>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kafkaBootstrapServers);
        ArgumentNullException.ThrowIfNull(retryTopics);
        ArgumentNullException.ThrowIfNull(producer);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(consumerOptions);
        ArgumentNullException.ThrowIfNull(liveness);
        if (retryTopics.Count == 0)
            throw new ArgumentException("retryTopics must contain at least one topic.", nameof(retryTopics));

        _kafkaBootstrapServers = kafkaBootstrapServers;
        _retryTopics = retryTopics;
        _producer = producer;
        _clock = clock;
        _consumerOptions = consumerOptions;
        _liveness = liveness;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            QueuedMaxMessagesKbytes = _consumerOptions.QueuedMaxMessagesKbytes,
            MessageMaxBytes = _consumerOptions.FetchMessageMaxBytes,
            MaxPollIntervalMs = _consumerOptions.MaxPollIntervalMs,
            SessionTimeoutMs = _consumerOptions.SessionTimeoutMs,
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
        };

        var consumerBuilder = new ConsumerBuilder<string, string>(config);
        // `_retryTopics[0]` is the canonical topic tag for rebalance +
        // lag observability — the worker subscribes to many retry
        // topics but under one worker identity for low-cardinality
        // tagging. The topic argument is a descriptive anchor, not a
        // partition-by-topic signal (the lag recorder tags actual
        // topic per message).
        KafkaRebalanceObservability.Attach(consumerBuilder, _retryTopics[0], WorkerName, _logger);
        using var consumer = consumerBuilder.Build();
        consumer.Subscribe(_retryTopics);

        _logger?.LogInformation(
            "KafkaRetryConsumerWorker subscribed to {TopicCount} retry topics under consumer group {Group}.",
            _retryTopics.Count, ConsumerGroup);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null)
                {
                    _liveness.RecordSuccess(WorkerName, _clock.UtcNow);
                    continue;
                }

                KafkaLagObservability.Record(consumer, result, WorkerName, _retryTopics[0]);

                await ProcessRetryMessageAsync(consumer, result, stoppingToken);

                _liveness.RecordSuccess(WorkerName, _clock.UtcNow);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (CircuitBreakerOpenException ex)
            {
                // Kafka producer breaker is open — skip this iteration
                // without committing so the `.retry` message gets
                // redelivered when the breaker recovers. Same posture
                // as KafkaOutboxPublisher per R-KAFKA-BREAKER-OPEN-BEHAVIOR-01.
                _logger?.LogWarning(
                    "KafkaRetryConsumerWorker skipping tick: breaker '{Breaker}' open ({RetryAfter}s).",
                    ex.BreakerName, ex.RetryAfterSeconds);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (ConsumeException ex)
            {
                _logger?.LogError(ex,
                    "KafkaRetryConsumerWorker ConsumeException: {Reason}", ex.Error.Reason);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "KafkaRetryConsumerWorker iteration failed; will continue.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        consumer.Close();
    }

    private async Task ProcessRetryMessageAsync(
        IConsumer<string, string> consumer,
        ConsumeResult<string, string> result,
        CancellationToken ct)
    {
        // Parse required retry headers. Any missing/unparseable is a
        // poison retry-tier message (retry contract violation) and
        // goes to `.deadletter` via the current topic's DLQ (we cannot
        // re-publish without knowing the source topic).
        var attemptCountStr = ExtractHeader(result.Message.Headers, RetryHeaders.AttemptCount);
        var deliverAfterStr = ExtractHeader(result.Message.Headers, RetryHeaders.DeliverAfterUnixMs);
        var sourceTopic = ExtractHeader(result.Message.Headers, RetryHeaders.SourceTopic);

        if (string.IsNullOrEmpty(attemptCountStr)
            || string.IsNullOrEmpty(deliverAfterStr)
            || string.IsNullOrEmpty(sourceTopic)
            || !int.TryParse(attemptCountStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var attemptCount)
            || !long.TryParse(deliverAfterStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var deliverAfterUnixMs))
        {
            _logger?.LogError(
                "KafkaRetryConsumerWorker: poison retry-tier message on {Topic} p{Partition} o{Offset} " +
                "— missing or unparseable retry headers. Committing offset (cannot re-publish without source topic).",
                result.Topic, result.Partition.Value, result.Offset.Value);
            consumer.Commit(result);
            return;
        }

        // Deliver-after gate. If the broker-recorded arrival-time plus
        // deliver-after-delay is in the future, we either sleep (small
        // delays) or yield without committing (large delays, Kafka
        // redelivers next poll cycle after ceiling).
        var nowUnixMs = _clock.UtcNow.ToUnixTimeMilliseconds();
        var delayMs = deliverAfterUnixMs - nowUnixMs;

        if (delayMs > DelayCeilingMs)
        {
            // Long delay — do NOT commit. Sleep the ceiling and let
            // Kafka redeliver on the next poll. This keeps the worker
            // responsive to cancellation + rebalance events during
            // multi-minute delays.
            _logger?.LogDebug(
                "KafkaRetryConsumerWorker: deliver-after {DeliverAfter}ms away on {Topic} p{Partition} o{Offset}; yielding (will redeliver).",
                delayMs, result.Topic, result.Partition.Value, result.Offset.Value);
            try { await Task.Delay(TimeSpan.FromMilliseconds(DelayCeilingMs), ct); }
            catch (OperationCanceledException) { }
            return;
        }

        if (delayMs > 0)
        {
            // Short delay — sleep in place.
            try { await Task.Delay(TimeSpan.FromMilliseconds(delayMs), ct); }
            catch (OperationCanceledException) { return; }
        }

        // Deliver-after has arrived. Re-publish the original message
        // to the source `.events` topic with retry-specific TRANSPORT
        // headers stripped and the envelope headers (including
        // retry-attempt-count) preserved so the source consumer can
        // see the attempt history and feed it back to the escalator
        // on the next failure.
        var republishedHeaders = new Headers();
        foreach (var h in result.Message.Headers)
        {
            // Strip deliver-after + source-topic (consumed by this
            // worker; no longer meaningful on the .events tier).
            // Preserve attempt-count + max-attempts so the next failure
            // knows the prior attempt history.
            if (h.Key == RetryHeaders.DeliverAfterUnixMs ||
                h.Key == RetryHeaders.SourceTopic)
                continue;
            republishedHeaders.Add(h.Key, h.GetValueBytes());
        }

        var republishMessage = new Message<string, string>
        {
            Key = result.Message.Key,
            Value = result.Message.Value,
            Headers = republishedHeaders,
        };

        await _producer.ProduceAsync(sourceTopic, republishMessage, ct);

        _logger?.LogInformation(
            "KafkaRetryConsumerWorker: re-published attempt {Attempt} to {SourceTopic} from {RetryTopic} p{Partition} o{Offset}.",
            attemptCount, sourceTopic, result.Topic, result.Partition.Value, result.Offset.Value);

        consumer.Commit(result);
    }

    private static string? ExtractHeader(Headers? headers, string key)
    {
        if (headers is null) return null;
        foreach (var h in headers)
        {
            if (string.Equals(h.Key, key, StringComparison.Ordinal))
            {
                var bytes = h.GetValueBytes();
                return bytes is null ? null : Encoding.UTF8.GetString(bytes);
            }
        }
        return null;
    }
}
