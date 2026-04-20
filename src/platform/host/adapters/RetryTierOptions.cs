namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.A.3d — declared configuration for the retry tier. Bound from
/// configuration in <c>KafkaInfrastructureModule.AddMessaging</c>:
/// <list type="bullet">
///   <item><c>Kafka:Retry:MaxAttempts</c> — ceiling for
///         <c>retry-attempt-count</c>. When the escalator would publish
///         an attempt beyond this value, it routes to <c>.deadletter</c>
///         instead of <c>.retry</c>.</item>
///   <item><c>Kafka:Retry:BaseBackoffSeconds</c> — delay for attempt 1.
///         Subsequent attempts follow exponential backoff (2× per
///         attempt) clamped by <see cref="MaxBackoff"/>.</item>
///   <item><c>Kafka:Retry:MaxBackoffSeconds</c> — ceiling for the
///         exponential backoff; jitter (+0 to +25%) may slightly
///         exceed this value.</item>
/// </list>
/// </summary>
public sealed record RetryTierOptions(
    int MaxAttempts,
    TimeSpan BaseBackoff,
    TimeSpan MaxBackoff);
