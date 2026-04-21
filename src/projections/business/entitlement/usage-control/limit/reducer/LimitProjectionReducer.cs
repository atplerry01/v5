using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Limit;

namespace Whycespace.Projections.Business.Entitlement.UsageControl.Limit.Reducer;

public static class LimitProjectionReducer
{
    public static LimitReadModel Apply(LimitReadModel state, LimitCreatedEventSchema e) =>
        state with
        {
            LimitId = e.AggregateId,
            SubjectId = e.SubjectId,
            ThresholdValue = e.ThresholdValue,
            Status = "Defined",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static LimitReadModel Apply(LimitReadModel state, LimitEnforcedEventSchema e) =>
        state with
        {
            LimitId = e.AggregateId,
            Status = "Enforced"
        };

    public static LimitReadModel Apply(LimitReadModel state, LimitBreachedEventSchema e) =>
        state with
        {
            LimitId = e.AggregateId,
            ObservedValue = e.ObservedValue,
            Status = "Breached"
        };
}
