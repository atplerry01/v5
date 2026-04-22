namespace Whycespace.Shared.Contracts.Events.Control.Observability.SystemAlert;

public sealed record SystemAlertDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string MetricDefinitionId,
    string ConditionExpression,
    string Severity);

public sealed record SystemAlertRetiredEventSchema(
    Guid AggregateId);
