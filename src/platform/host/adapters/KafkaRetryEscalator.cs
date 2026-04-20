using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.A.3d / R-RETRY-ESCALATOR-01 — single canonical seam for
/// transitioning a failed message from its source <c>.events</c> topic
/// into either the <c>.retry</c> tier (attempt budget remaining) or the
/// <c>.deadletter</c> tier (budget exhausted).
///
/// Replaces direct `.deadletter` publishes from consumer handler-throw
/// paths with an attempt-counted escalation that adds exponential
/// backoff + deterministic jitter. Poison-message paths (absent /
/// malformed headers, JSON deserialize) continue to route direct to
/// `.deadletter` since those are never retryable.
///
/// The `.retry → .events` re-publish (when deliver-after arrives) is
/// also routed through this escalator by <see cref="KafkaRetryConsumerWorker"/>
/// — same seam, caller determines whether the escalation is "retry"
/// (first attempt or subsequent attempts within budget) or
/// "deadletter" (budget exhausted).
///
/// Replay determinism (R-RETRY-DELIVER-AFTER-DETERMINISM-01): deliver-after
/// is computed from <see cref="IClock.UtcNow"/> + deterministic exponential
/// backoff + deterministic jitter seeded via <see cref="IRandomProvider.NextDouble"/>.
/// No <c>DateTime.UtcNow</c>, <c>System.Random</c>, or <c>Stopwatch</c>-derived
/// values appear in the delay formula.
/// </summary>
public static class KafkaRetryEscalator
{
    /// <summary>
    /// Outcome returned by <see cref="EscalateAsync"/> — carries the
    /// tier transition result for caller logging / event emission.
    /// </summary>
    public readonly record struct RetryEscalationOutcome(
        RetryEscalationTier Tier,
        int NextAttemptCount,
        DateTimeOffset DeliverAfterUtc,
        string TargetTopic);

    public enum RetryEscalationTier
    {
        /// <summary>Published to the `.retry` topic; retry consumer will re-publish to source when deliver-after arrives.</summary>
        Retry = 0,

        /// <summary>Attempt budget exhausted; published to the `.deadletter` topic as terminal.</summary>
        DeadLetter = 1,
    }

    /// <summary>
    /// Escalates a failed message. If <paramref name="attemptCount"/> + 1
    /// exceeds <paramref name="maxAttempts"/>, the message is routed to
    /// the `.deadletter` tier. Otherwise it is routed to `.retry` with
    /// the four R-RETRY-HEADERS-01 headers added (original headers
    /// preserved).
    /// </summary>
    /// <param name="producer">Caller-owned Kafka producer.</param>
    /// <param name="topicNameResolver">Shared resolver (D2 LOCKED tier
    /// convention).</param>
    /// <param name="sourceTopic">The canonical <c>.events</c> topic this
    /// message was originally consumed from.</param>
    /// <param name="original">The original message (headers + payload).
    /// Headers are preserved unchanged; retry-tier headers are added.</param>
    /// <param name="eventId">Event identifier — used as the retry-seed
    /// salt so jitter is replay-deterministic.</param>
    /// <param name="failureReason">Human-readable failure cause; stored
    /// as <c>dlq-reason</c> header only on the deadletter branch.</param>
    /// <param name="attemptCount">Previous attempt count (0 for
    /// first-failure escalation from a handler, ≥1 when the retry
    /// consumer re-escalates).</param>
    /// <param name="maxAttempts">Ceiling for attempt count. Beyond
    /// this value, escalation routes to deadletter.</param>
    /// <param name="baseBackoff">First-attempt backoff (attempt 1).</param>
    /// <param name="maxBackoff">Ceiling for exponential backoff; the
    /// jitter may slightly exceed this due to the +0 to +25% spread.</param>
    /// <param name="randomProvider">R1 seam — deterministic jitter source.</param>
    /// <param name="clock">R1 seam — deterministic time source.</param>
    /// <param name="kafkaBreaker">R-KAFKA-BREAKER-01 shared
    /// <c>"kafka-producer"</c> breaker. Required (not optional) —
    /// every producer write site is breaker-protected by R2.A.D.3b.</param>
    /// <param name="logger">Null-tolerant structured logger.</param>
    /// <param name="cancellationToken">Standard cancellation token.</param>
    public static async Task<RetryEscalationOutcome> EscalateAsync(
        IProducer<string, string> producer,
        TopicNameResolver topicNameResolver,
        string sourceTopic,
        Message<string, string> original,
        Guid eventId,
        string failureReason,
        int attemptCount,
        int maxAttempts,
        TimeSpan baseBackoff,
        TimeSpan maxBackoff,
        IRandomProvider randomProvider,
        IClock clock,
        ICircuitBreaker kafkaBreaker,
        ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(producer);
        ArgumentNullException.ThrowIfNull(topicNameResolver);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTopic);
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(randomProvider);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(kafkaBreaker);
        if (attemptCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(attemptCount), attemptCount, "attemptCount must be non-negative.");
        if (maxAttempts < 1)
            throw new ArgumentOutOfRangeException(
                nameof(maxAttempts), maxAttempts, "maxAttempts must be at least 1.");
        if (baseBackoff <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(baseBackoff), baseBackoff, "baseBackoff must be positive.");
        if (maxBackoff < baseBackoff)
            throw new ArgumentOutOfRangeException(
                nameof(maxBackoff), maxBackoff, "maxBackoff must be >= baseBackoff.");

        var nextAttempt = attemptCount + 1;

        // Exhaustion branch: route to `.deadletter` with attempt-count
        // in dlq-reason so operators can diagnose "exhausted" vs
        // "poisoned-on-first-sight" cases.
        if (nextAttempt > maxAttempts)
        {
            var deadletterTopic = topicNameResolver.ResolveDeadLetter(sourceTopic);
            var dlqHeaders = CloneOriginalHeaders(original);
            dlqHeaders.Add(
                "dlq-reason",
                Encoding.UTF8.GetBytes($"retry_exhausted:attempts={attemptCount}:max={maxAttempts}:cause={failureReason}"));
            dlqHeaders.Add(
                "dlq-source-topic",
                Encoding.UTF8.GetBytes(sourceTopic));

            var dlqMessage = new Message<string, string>
            {
                Key = original.Key,
                Value = original.Value,
                Headers = dlqHeaders,
            };

            await kafkaBreaker.ExecuteAsync(
                ct => producer.ProduceAsync(deadletterTopic, dlqMessage, ct),
                cancellationToken);

            logger?.LogWarning(
                "Retry escalator: event {EventId} exhausted after {Attempts} attempts (max={Max}) on {SourceTopic}; routed to {DeadletterTopic}. Cause: {Reason}",
                eventId, attemptCount, maxAttempts, sourceTopic, deadletterTopic, failureReason);

            return new RetryEscalationOutcome(
                Tier: RetryEscalationTier.DeadLetter,
                NextAttemptCount: nextAttempt,
                DeliverAfterUtc: clock.UtcNow,
                TargetTopic: deadletterTopic);
        }

        // Retry branch: compute deterministic deliver-after and publish
        // to `.retry` with the four R-RETRY-HEADERS-01 headers.
        var retryTopic = topicNameResolver.ResolveRetry(sourceTopic);
        var deliverAfter = ComputeDeliverAfter(
            sourceTopic, eventId, nextAttempt, baseBackoff, maxBackoff,
            randomProvider, clock);

        var retryMessageHeaders = CloneOriginalHeaders(original);
        retryMessageHeaders.Add(
            RetryHeaders.AttemptCount,
            Encoding.UTF8.GetBytes(nextAttempt.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        retryMessageHeaders.Add(
            RetryHeaders.MaxAttempts,
            Encoding.UTF8.GetBytes(maxAttempts.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        retryMessageHeaders.Add(
            RetryHeaders.DeliverAfterUnixMs,
            Encoding.UTF8.GetBytes(deliverAfter.ToUnixTimeMilliseconds()
                .ToString(System.Globalization.CultureInfo.InvariantCulture)));
        retryMessageHeaders.Add(
            RetryHeaders.SourceTopic,
            Encoding.UTF8.GetBytes(sourceTopic));

        var retryMessage = new Message<string, string>
        {
            Key = original.Key,
            Value = original.Value,
            Headers = retryMessageHeaders,
        };

        await kafkaBreaker.ExecuteAsync(
            ct => producer.ProduceAsync(retryTopic, retryMessage, ct),
            cancellationToken);

        logger?.LogInformation(
            "Retry escalator: event {EventId} attempt {Attempt} of {Max} on {SourceTopic}; scheduled on {RetryTopic} for {DeliverAfter:O}. Cause: {Reason}",
            eventId, nextAttempt, maxAttempts, sourceTopic, retryTopic, deliverAfter, failureReason);

        return new RetryEscalationOutcome(
            Tier: RetryEscalationTier.Retry,
            NextAttemptCount: nextAttempt,
            DeliverAfterUtc: deliverAfter,
            TargetTopic: retryTopic);
    }

    /// <summary>
    /// Replay-deterministic deliver-after computation. The same
    /// <paramref name="sourceTopic"/>, <paramref name="eventId"/>,
    /// <paramref name="attemptCount"/> always produce the same value
    /// given a deterministic clock + deterministic random provider.
    ///
    /// Public for unit-testability — pure function over its
    /// deterministic inputs; no side effects, no hidden state.
    /// </summary>
    public static DateTimeOffset ComputeDeliverAfter(
        string sourceTopic,
        Guid eventId,
        int attemptCount,
        TimeSpan baseBackoff,
        TimeSpan maxBackoff,
        IRandomProvider randomProvider,
        IClock clock)
    {
        // Exponential backoff: base × 2^(attempt - 1), clamped to maxBackoff.
        // attempt=1 → base × 1, attempt=2 → base × 2, attempt=3 → base × 4, ...
        // Using double to avoid overflow on large attempt counts.
        var exponent = Math.Pow(2.0, Math.Max(0, attemptCount - 1));
        var baseMs = baseBackoff.TotalMilliseconds * exponent;
        var cappedMs = Math.Min(baseMs, maxBackoff.TotalMilliseconds);

        // Jitter: +0 to +25% of the capped value, seeded deterministically.
        // Seed format matches the R1/R2 determinism contract:
        //   "{sourceTopic}:{eventId}:retry:{attemptCount}"
        var jitterSeed = string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"{sourceTopic}:{eventId}:retry:{attemptCount}");
        var jitterFactor = randomProvider.NextDouble(jitterSeed);
        var jitteredMs = cappedMs * (1.0 + jitterFactor * 0.25);

        return clock.UtcNow + TimeSpan.FromMilliseconds(jitteredMs);
    }

    private static Headers CloneOriginalHeaders(Message<string, string> original)
    {
        var headers = new Headers();
        if (original.Headers is null) return headers;
        foreach (var h in original.Headers)
        {
            headers.Add(h.Key, h.GetValueBytes());
        }
        return headers;
    }
}
