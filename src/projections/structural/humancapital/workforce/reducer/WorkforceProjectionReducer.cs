using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Workforce;
using Whycespace.Shared.Contracts.Structural.Humancapital.Workforce;

namespace Whycespace.Projections.Structural.Humancapital.Workforce.Reducer;

public static class WorkforceProjectionReducer
{
    public static WorkforceReadModel Apply(WorkforceReadModel state, WorkforceCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            WorkforceId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
