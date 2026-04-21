using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Performance;
using Whycespace.Shared.Contracts.Structural.Humancapital.Performance;

namespace Whycespace.Projections.Structural.Humancapital.Performance.Reducer;

public static class PerformanceProjectionReducer
{
    public static PerformanceReadModel Apply(PerformanceReadModel state, PerformanceCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            PerformanceId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
