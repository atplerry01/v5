using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Clause;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Clause.Reducer;

public static class ClauseProjectionReducer
{
    public static ClauseReadModel Apply(ClauseReadModel state, ClauseCreatedEventSchema e) =>
        state with
        {
            ClauseId = e.AggregateId,
            ClauseType = e.ClauseType,
            Status = "Draft",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static ClauseReadModel Apply(ClauseReadModel state, ClauseActivatedEventSchema e) =>
        state with
        {
            ClauseId = e.AggregateId,
            Status = "Active"
        };

    public static ClauseReadModel Apply(ClauseReadModel state, ClauseSupersededEventSchema e) =>
        state with
        {
            ClauseId = e.AggregateId,
            Status = "Superseded"
        };
}
