using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Incentive;
using Whycespace.Shared.Contracts.Structural.Humancapital.Incentive;

namespace Whycespace.Projections.Structural.Humancapital.Incentive.Reducer;

public static class IncentiveProjectionReducer
{
    public static IncentiveReadModel Apply(IncentiveReadModel state, IncentiveCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            IncentiveId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
