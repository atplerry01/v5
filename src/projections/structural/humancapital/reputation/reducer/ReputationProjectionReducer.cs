using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Reputation;
using Whycespace.Shared.Contracts.Structural.Humancapital.Reputation;

namespace Whycespace.Projections.Structural.Humancapital.Reputation.Reducer;

public static class ReputationProjectionReducer
{
    public static ReputationReadModel Apply(ReputationReadModel state, ReputationCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            ReputationId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
