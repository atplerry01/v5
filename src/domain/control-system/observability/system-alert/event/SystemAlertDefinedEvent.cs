using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemAlert;

public sealed record SystemAlertDefinedEvent(
    SystemAlertId Id,
    string Name,
    string MetricDefinitionId,
    string ConditionExpression,
    AlertSeverity Severity) : DomainEvent;
