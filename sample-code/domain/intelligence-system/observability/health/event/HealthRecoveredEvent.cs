using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Health;

public sealed record HealthRecoveredEvent(
    string ComponentId) : DomainEvent;
