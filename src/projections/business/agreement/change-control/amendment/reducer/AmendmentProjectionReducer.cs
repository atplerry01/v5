using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Amendment;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Amendment.Reducer;

public static class AmendmentProjectionReducer
{
    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentCreatedEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            TargetId = e.TargetId,
            Status = "Draft",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentAppliedEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            Status = "Applied"
        };

    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentRevertedEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            Status = "Reverted"
        };
}
