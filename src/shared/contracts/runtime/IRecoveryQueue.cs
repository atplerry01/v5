namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Recovery queue for workflow steps that fail after retry exhaustion.
/// Instead of immediately compensating (which may be premature for
/// transient infrastructure failures), the step publishes a recovery
/// entry that will be retried by a dedicated recovery worker on a
/// longer interval.
///
/// ESCALATION PATH:
///   Step fails → graduated retry (immediate + backoff)
///     → retry exhausted → publish to recovery queue
///       → recovery worker retries on 30s/60s/120s intervals
///         → still failing → compensate
///
/// This prevents premature compensation for recoverable failures
/// (Postgres blip, Kafka partition rebalance, brief network partition)
/// while still guaranteeing eventual resolution.
///
/// The recovery queue is backed by the existing Kafka retry topic
/// infrastructure (whyce.{classification}.{context}.{domain}.retry).
/// </summary>
public interface IRecoveryQueue
{
    Task<bool> PublishAsync(RecoveryEntry entry, CancellationToken cancellationToken = default);
}

/// <summary>
/// A failed workflow step to be retried by the recovery worker.
/// Contains all the data needed to replay the step.
/// </summary>
public sealed record RecoveryEntry(
    Guid EntryId,
    string WorkflowName,
    string StepName,
    string SerializedState,
    string LastError,
    int AttemptCount,
    DateTimeOffset FailedAt,
    string Classification,
    string Context,
    string Domain);
