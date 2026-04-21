using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Stewardship;
using Whycespace.Shared.Contracts.Structural.Humancapital.Stewardship;

namespace Whycespace.Projections.Structural.Humancapital.Stewardship.Reducer;

public static class StewardshipProjectionReducer
{
    public static StewardshipReadModel Apply(StewardshipReadModel state, StewardshipCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            StewardshipId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
