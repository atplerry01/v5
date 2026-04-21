using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;

public sealed record CreateLimitCommand(
    Guid LimitId,
    Guid SubjectId,
    int ThresholdValue) : IHasAggregateId
{
    public Guid AggregateId => LimitId;
}

public sealed record EnforceLimitCommand(Guid LimitId) : IHasAggregateId
{
    public Guid AggregateId => LimitId;
}

public sealed record BreachLimitCommand(
    Guid LimitId,
    int ObservedValue) : IHasAggregateId
{
    public Guid AggregateId => LimitId;
}
