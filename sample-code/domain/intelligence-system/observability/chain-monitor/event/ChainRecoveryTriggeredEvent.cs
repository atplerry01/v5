namespace Whycespace.Domain.IntelligenceSystem.Observability.ChainMonitor;

/// <summary>
/// Observability event — emitted when chain recovery is triggered.
/// NOT a domain chain event. Used for monitoring and audit only.
/// </summary>
public sealed record ChainRecoveryTriggeredEvent(
    string RecoveryMode,
    long BlocksProcessed,
    bool IsClean,
    int CorruptedSegmentCount) : DomainEvent;
