using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Alert;

public sealed record AlertResolvedEvent(
    Guid AlertId,
    string MetricType,
    decimal MetricValue
) : DomainEvent;
