using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Plan;

namespace Whycespace.Projections.Business.Offering.CommercialShape.Plan.Reducer;

public static class PlanProjectionReducer
{
    public static PlanReadModel Apply(PlanReadModel state, PlanDraftedEventSchema e) =>
        state with
        {
            PlanId = e.AggregateId,
            PlanName = e.PlanName,
            PlanTier = e.PlanTier,
            Status = "Draft"
        };

    public static PlanReadModel Apply(PlanReadModel state, PlanActivatedEventSchema e) =>
        state with
        {
            PlanId = e.AggregateId,
            Status = "Active"
        };

    public static PlanReadModel Apply(PlanReadModel state, PlanDeprecatedEventSchema e) =>
        state with
        {
            PlanId = e.AggregateId,
            Status = "Deprecated"
        };

    public static PlanReadModel Apply(PlanReadModel state, PlanArchivedEventSchema e) =>
        state with
        {
            PlanId = e.AggregateId,
            Status = "Archived"
        };
}
