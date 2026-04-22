using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Observability.SystemAlert;

public sealed record DefineSystemAlertCommand(
    Guid AlertId,
    string Name,
    string MetricDefinitionId,
    string ConditionExpression,
    string Severity) : IHasAggregateId
{
    public Guid AggregateId => AlertId;
}

public sealed record RetireSystemAlertCommand(
    Guid AlertId) : IHasAggregateId
{
    public Guid AggregateId => AlertId;
}
