using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Operator;
using Whycespace.Shared.Contracts.Structural.Humancapital.Operator;

namespace Whycespace.Projections.Structural.Humancapital.Operator.Reducer;

public static class OperatorProjectionReducer
{
    public static OperatorReadModel Apply(OperatorReadModel state, OperatorCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            OperatorId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
