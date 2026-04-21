using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.UsageRight;

namespace Whycespace.Projections.Business.Entitlement.UsageControl.UsageRight.Reducer;

public static class UsageRightProjectionReducer
{
    public static UsageRightReadModel Apply(UsageRightReadModel state, UsageRightCreatedEventSchema e) =>
        state with
        {
            UsageRightId = e.AggregateId,
            SubjectId = e.SubjectId,
            ReferenceId = e.ReferenceId,
            TotalUnits = e.TotalUnits,
            TotalUsed = 0,
            LastRecordId = Guid.Empty,
            Status = "Available",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static UsageRightReadModel Apply(UsageRightReadModel state, UsageRightUsedEventSchema e) =>
        state with
        {
            UsageRightId = e.AggregateId,
            TotalUsed = state.TotalUsed + e.UnitsUsed,
            LastRecordId = e.RecordId,
            Status = "InUse"
        };

    public static UsageRightReadModel Apply(UsageRightReadModel state, UsageRightConsumedEventSchema e) =>
        state with
        {
            UsageRightId = e.AggregateId,
            Status = "Consumed"
        };
}
