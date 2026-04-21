using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Governance;
using Whycespace.Shared.Contracts.Structural.Humancapital.Governance;

namespace Whycespace.Projections.Structural.Humancapital.Governance.Reducer;

public static class GovernanceProjectionReducer
{
    public static GovernanceReadModel Apply(GovernanceReadModel state, GovernanceCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            GovernanceId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
