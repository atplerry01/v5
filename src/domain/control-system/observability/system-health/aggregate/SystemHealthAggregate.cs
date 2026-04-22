using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemHealth;

public sealed class SystemHealthAggregate : AggregateRoot
{
    public SystemHealthId Id { get; private set; }
    public string ComponentName { get; private set; } = string.Empty;
    public HealthStatus Status { get; private set; }
    public DateTimeOffset LastEvaluatedAt { get; private set; }

    private SystemHealthAggregate() { }

    public static SystemHealthAggregate Evaluate(
        SystemHealthId id,
        string componentName,
        HealthStatus status,
        DateTimeOffset evaluatedAt)
    {
        Guard.Against(string.IsNullOrEmpty(componentName), SystemHealthErrors.ComponentNameMustNotBeEmpty().Message);

        var aggregate = new SystemHealthAggregate();
        aggregate.RaiseDomainEvent(new SystemHealthEvaluatedEvent(id, componentName, status, evaluatedAt));
        return aggregate;
    }

    public void RecordDegradation(HealthStatus newStatus, string reason, DateTimeOffset occurredAt)
    {
        Guard.Against(Status == newStatus, SystemHealthErrors.HealthAlreadyInStatus(newStatus).Message);
        Guard.Against(newStatus == HealthStatus.Healthy, "Use Restore() to record recovery.");
        Guard.Against(string.IsNullOrEmpty(reason), SystemHealthErrors.ReasonMustNotBeEmptyOnDegradation().Message);

        RaiseDomainEvent(new SystemHealthDegradedEvent(Id, newStatus, reason, occurredAt));
    }

    public void Restore(DateTimeOffset restoredAt)
    {
        Guard.Against(Status == HealthStatus.Healthy, SystemHealthErrors.HealthAlreadyInStatus(HealthStatus.Healthy).Message);

        RaiseDomainEvent(new SystemHealthRestoredEvent(Id, restoredAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SystemHealthEvaluatedEvent e:
                Id = e.Id;
                ComponentName = e.ComponentName;
                Status = e.Status;
                LastEvaluatedAt = e.EvaluatedAt;
                break;
            case SystemHealthDegradedEvent e:
                Status = e.NewStatus;
                LastEvaluatedAt = e.OccurredAt;
                break;
            case SystemHealthRestoredEvent e:
                Status = HealthStatus.Healthy;
                LastEvaluatedAt = e.RestoredAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "SystemHealth must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(ComponentName), "SystemHealth must have a non-empty ComponentName.");
    }
}
