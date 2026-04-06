namespace Whycespace.Domain.IntelligenceSystem.Observability.ChainMonitor;

/// <summary>
/// Observability event — emitted when a chain anomaly is detected.
/// Anomaly types: hash_mismatch, duplicate_correlation, fork_attempt, continuity_break.
/// NOT a domain chain event. Used for monitoring and alerting only.
/// </summary>
public sealed record ChainAnomalyDetectedEvent(
    string AnomalyType,
    string Description,
    long AtSequenceNumber,
    string BlockId) : DomainEvent;
