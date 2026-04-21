using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Eligibility;
using Whycespace.Shared.Contracts.Structural.Humancapital.Eligibility;

namespace Whycespace.Projections.Structural.Humancapital.Eligibility.Reducer;

public static class EligibilityProjectionReducer
{
    public static EligibilityReadModel Apply(EligibilityReadModel state, EligibilityCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            EligibilityId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
