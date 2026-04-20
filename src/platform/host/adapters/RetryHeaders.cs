using System.Globalization;
using Confluent.Kafka;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.A.3d / R-RETRY-HEADERS-01 — canonical Kafka header names for the
/// <c>.retry</c> tier. Single source of truth; every producer and
/// consumer of a <c>.retry</c> topic MUST reference these constants
/// rather than inlining string literals.
///
/// Header contract summary (all UTF-8 encoded):
/// <list type="bullet">
///   <item><see cref="AttemptCount"/> — 1-based integer counting the
///         number of failed processing attempts that produced this
///         retry-tier message.</item>
///   <item><see cref="MaxAttempts"/> — integer ceiling for
///         <see cref="AttemptCount"/> beyond which the escalator routes
///         to <c>.deadletter</c> instead of <c>.retry</c>. Carried
///         per-message so per-event-type overrides are possible in
///         future passes without changing the escalator contract.</item>
///   <item><see cref="DeliverAfterUnixMs"/> — Unix-epoch-milliseconds
///         value; the retry consumer re-publishes to the source
///         <c>.events</c> topic at-or-after this instant. Computed
///         deterministically at escalation time via
///         <c>IClock.UtcNow + ExponentialBackoff(attempt, base, max, jitter)</c>
///         with jitter sourced from <c>IRandomProvider</c>.</item>
///   <item><see cref="SourceTopic"/> — canonical <c>.events</c> topic
///         name the retry consumer re-publishes to when deliver-after
///         arrives. Carried because the retry consumer subscribes to
///         many <c>.retry</c> topics under a single consumer group.</item>
/// </list>
///
/// Original envelope headers (<c>event-id</c>, <c>aggregate-id</c>,
/// <c>event-type</c>, <c>correlation-id</c>, <c>causation-id</c>) are
/// preserved unchanged across retry-tier transitions — the retry tier
/// is a transport mechanism, not a re-envelope step.
/// </summary>
public static class RetryHeaders
{
    public const string AttemptCount = "retry-attempt-count";
    public const string MaxAttempts = "retry-max-attempts";
    public const string DeliverAfterUnixMs = "retry-deliver-after-unix-ms";
    public const string SourceTopic = "retry-source-topic";

    /// <summary>
    /// R2.A.3d / R2.A.3d Phase B: extract the retry-attempt-count header
    /// value. Returns 0 when the header is absent (first failure) or
    /// unparseable. The escalator increments this value when publishing
    /// the next retry-tier message — so priorAttempt=0 means "this is
    /// the first failure" and the escalator writes attempt-count=1 on
    /// the `.retry` publish.
    ///
    /// Shared by every consumer worker that calls
    /// <see cref="KafkaRetryEscalator.EscalateAsync"/> on handler-throw
    /// paths. Centralising the parse keeps the `"header is absent →
    /// treat as 0"` rule authoritative in a single place.
    /// </summary>
    public static int ReadPriorAttemptCount(Headers? headers)
    {
        if (headers is null) return 0;
        foreach (var h in headers)
        {
            if (!string.Equals(h.Key, AttemptCount, StringComparison.Ordinal))
                continue;
            var bytes = h.GetValueBytes();
            if (bytes is null || bytes.Length == 0) return 0;
            var raw = System.Text.Encoding.UTF8.GetString(bytes);
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
                ? n
                : 0;
        }
        return 0;
    }
}
