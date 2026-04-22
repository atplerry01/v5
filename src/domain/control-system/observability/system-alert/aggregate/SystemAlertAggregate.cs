using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemAlert;

public sealed class SystemAlertAggregate : AggregateRoot
{
    public SystemAlertId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string MetricDefinitionId { get; private set; } = string.Empty;
    public string ConditionExpression { get; private set; } = string.Empty;
    public AlertSeverity Severity { get; private set; }
    public bool IsRetired { get; private set; }

    private SystemAlertAggregate() { }

    public static SystemAlertAggregate Define(
        SystemAlertId id,
        string name,
        string metricDefinitionId,
        string conditionExpression,
        AlertSeverity severity)
    {
        Guard.Against(string.IsNullOrEmpty(name), "SystemAlert name must not be empty.");
        Guard.Against(string.IsNullOrEmpty(metricDefinitionId), "SystemAlert requires a metricDefinitionId.");
        Guard.Against(string.IsNullOrEmpty(conditionExpression), "SystemAlert condition must not be empty.");

        var aggregate = new SystemAlertAggregate();
        aggregate.RaiseDomainEvent(
            new SystemAlertDefinedEvent(id, name, metricDefinitionId, conditionExpression, severity));
        return aggregate;
    }

    public void Retire()
    {
        Guard.Against(IsRetired, "SystemAlert is already retired.");
        RaiseDomainEvent(new SystemAlertRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SystemAlertDefinedEvent e:
                Id = e.Id; Name = e.Name; MetricDefinitionId = e.MetricDefinitionId;
                ConditionExpression = e.ConditionExpression; Severity = e.Severity;
                break;
            case SystemAlertRetiredEvent:
                IsRetired = true;
                break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "SystemAlert must have an Id.");
}
