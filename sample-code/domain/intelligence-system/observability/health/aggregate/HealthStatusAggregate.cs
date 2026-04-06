using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Health;

public sealed class HealthStatusAggregate : AggregateRoot
{
    public ComponentId ComponentId { get; private set; }
    public HealthState Status { get; private set; } = HealthState.Healthy;
    public DateTimeOffset LastCheckedAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private HealthStatusAggregate() { }

    public static HealthStatusAggregate Create(Guid healthStatusId, ComponentId componentId, DateTimeOffset timestamp)
    {
        var health = new HealthStatusAggregate
        {
            Id = healthStatusId,
            ComponentId = componentId,
            Status = HealthState.Healthy,
            LastCheckedAt = timestamp,
            Reason = string.Empty
        };

        return health;
    }

    public void UpdateStatus(HealthState newStatus, string reason, DateTimeOffset timestamp)
    {
        var previousStatus = Status;
        Status = newStatus;
        Reason = reason;
        LastCheckedAt = timestamp;

        if (newStatus == HealthState.Degraded || newStatus == HealthState.Unhealthy)
        {
            if (previousStatus == HealthState.Healthy)
                RaiseDomainEvent(new HealthDegradedEvent(ComponentId, reason));
        }

        if (newStatus == HealthState.Healthy && previousStatus != HealthState.Healthy)
        {
            RaiseDomainEvent(new HealthRecoveredEvent(ComponentId));
        }
    }
}
