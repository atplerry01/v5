using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Alert;

public sealed class AlertDefinitionAggregate : AggregateRoot
{
    public AlertId AlertId { get; private set; }
    public string MetricType { get; private set; } = null!;
    public ThresholdValue Threshold { get; private set; }
    public AlertCondition Condition { get; private set; } = null!;
    public AlertSeverity Severity { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private AlertDefinitionAggregate() { }

    public static AlertDefinitionAggregate Create(
        Guid alertDefinitionId,
        AlertId alertId,
        string metricType,
        ThresholdValue threshold,
        AlertCondition condition,
        AlertSeverity severity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricType);
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(severity);

        var alert = new AlertDefinitionAggregate
        {
            Id = alertDefinitionId,
            AlertId = alertId,
            MetricType = metricType,
            Threshold = threshold,
            Condition = condition,
            Severity = severity,
            IsActive = true
        };

        return alert;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public bool Evaluate(decimal metricValue)
    {
        if (!IsActive) return false;

        var triggered = Condition.Evaluate(metricValue, Threshold.Value);

        if (triggered)
        {
            RaiseDomainEvent(new AlertTriggeredEvent(
                AlertId: AlertId,
                MetricType: MetricType,
                MetricValue: metricValue,
                Threshold: Threshold.Value,
                Condition: Condition.Operator,
                Severity: Severity.Value
            ));
        }
        else
        {
            RaiseDomainEvent(new AlertResolvedEvent(
                AlertId: AlertId,
                MetricType: MetricType,
                MetricValue: metricValue
            ));
        }

        return triggered;
    }
}
