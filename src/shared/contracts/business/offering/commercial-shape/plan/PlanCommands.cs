using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;

public sealed record DraftPlanCommand(
    Guid PlanId,
    string PlanName,
    string PlanTier) : IHasAggregateId
{
    public Guid AggregateId => PlanId;
}

public sealed record ActivatePlanCommand(Guid PlanId) : IHasAggregateId
{
    public Guid AggregateId => PlanId;
}

public sealed record DeprecatePlanCommand(Guid PlanId) : IHasAggregateId
{
    public Guid AggregateId => PlanId;
}

public sealed record ArchivePlanCommand(Guid PlanId) : IHasAggregateId
{
    public Guid AggregateId => PlanId;
}
