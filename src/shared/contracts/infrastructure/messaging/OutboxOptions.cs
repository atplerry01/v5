namespace Whyce.Shared.Contracts.Infrastructure.Messaging;

/// <summary>
/// Tunable behavior for the Postgres-to-Kafka outbox relay.
///
/// phase1.6-S1.5 (OUTBOX-CONFIG-01): externalises the previously hardcoded
/// MAX_RETRY constant from KafkaOutboxPublisher. The composition root reads
/// values from configuration (env-var first, per CFG-R1/R2) and constructs
/// this record explicitly — there is no IOptions&lt;T&gt; indirection because
/// no other code in the codebase uses one and adding it would break the
/// "config explicit at the composition root" pattern.
///
/// Defaults are intentionally conservative and match the pre-S1.5 hardcoded
/// constant so that omitting the configuration key produces identical
/// runtime behavior. Any deployment that needs different tuning sets the
/// corresponding environment variable.
/// </summary>
public sealed record OutboxOptions
{
    /// <summary>
    /// Maximum number of publish attempts before a row is promoted to
    /// status='deadletter' and (if applicable) republished to the DLQ
    /// topic. Must be at least 1. Default 5 (matches the pre-S1.5
    /// hardcoded constant).
    /// </summary>
    public int MaxRetry { get; init; } = 5;
}
