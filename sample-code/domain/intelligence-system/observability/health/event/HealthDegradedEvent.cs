using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Health;

public sealed record HealthDegradedEvent(
    string ComponentId,
    string Reason) : DomainEvent;
