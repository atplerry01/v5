using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Observability.SystemMetric;

public sealed record DefineSystemMetricCommand(
    Guid MetricId,
    string Name,
    string Type,
    string Unit,
    IReadOnlyList<string> LabelNames) : IHasAggregateId
{
    public Guid AggregateId => MetricId;
}

public sealed record DeprecateSystemMetricCommand(
    Guid MetricId) : IHasAggregateId
{
    public Guid AggregateId => MetricId;
}
