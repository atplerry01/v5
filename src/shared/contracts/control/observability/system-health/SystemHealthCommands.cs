using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Observability.SystemHealth;

public sealed record EvaluateSystemHealthCommand(
    Guid HealthId,
    string ComponentName,
    string Status,
    DateTimeOffset EvaluatedAt) : IHasAggregateId
{
    public Guid AggregateId => HealthId;
}

public sealed record RecordSystemHealthDegradationCommand(
    Guid HealthId,
    string NewStatus,
    string Reason,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => HealthId;
}

public sealed record RestoreSystemHealthCommand(
    Guid HealthId,
    DateTimeOffset RestoredAt) : IHasAggregateId
{
    public Guid AggregateId => HealthId;
}
