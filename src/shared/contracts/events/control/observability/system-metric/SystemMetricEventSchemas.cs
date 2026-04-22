namespace Whycespace.Shared.Contracts.Events.Control.Observability.SystemMetric;

public sealed record SystemMetricDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string Type,
    string Unit,
    IReadOnlyList<string> LabelNames);

public sealed record SystemMetricDeprecatedEventSchema(
    Guid AggregateId);
