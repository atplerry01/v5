namespace Whycespace.Domain.IntelligenceSystem.Observability.ChainMonitor;

/// <summary>
/// Observability event — emitted when a chain health check completes.
/// NOT a domain chain event. Used for monitoring and alerting only.
/// </summary>
public sealed record ChainHealthCheckedEvent(
    bool IsHealthy,
    int BlockHeight,
    string CurrentHead,
    int IssueCount) : DomainEvent;
