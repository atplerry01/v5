using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sanction;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sanction;

namespace Whycespace.Projections.Structural.Humancapital.Sanction.Reducer;

public static class SanctionProjectionReducer
{
    public static SanctionReadModel Apply(SanctionReadModel state, SanctionCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SanctionId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
