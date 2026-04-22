namespace Whycespace.Shared.Contracts.Control.Observability.SystemAlert;

public sealed record SystemAlertReadModel
{
    public Guid AlertId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string MetricDefinitionId { get; init; } = string.Empty;
    public string ConditionExpression { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
