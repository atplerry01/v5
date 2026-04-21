using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;

public sealed record CreateUsageRightCommand(
    Guid UsageRightId,
    Guid SubjectId,
    Guid ReferenceId,
    int TotalUnits) : IHasAggregateId
{
    public Guid AggregateId => UsageRightId;
}

public sealed record UseUsageRightCommand(
    Guid UsageRightId,
    Guid RecordId,
    int UnitsUsed) : IHasAggregateId
{
    public Guid AggregateId => UsageRightId;
}

public sealed record ConsumeUsageRightCommand(Guid UsageRightId) : IHasAggregateId
{
    public Guid AggregateId => UsageRightId;
}
