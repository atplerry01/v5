using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Alert;

public sealed record AlertTriggeredEvent(
    Guid AlertId,
    string MetricType,
    decimal MetricValue,
    decimal Threshold,
    string Condition,
    string Severity
) : DomainEvent;
