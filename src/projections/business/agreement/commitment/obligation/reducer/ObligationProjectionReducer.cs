using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Obligation;

namespace Whycespace.Projections.Business.Agreement.Commitment.Obligation.Reducer;

public static class ObligationProjectionReducer
{
    public static ObligationReadModel Apply(ObligationReadModel state, ObligationCreatedEventSchema e) =>
        state with
        {
            ObligationId = e.AggregateId,
            Status = "Pending",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static ObligationReadModel Apply(ObligationReadModel state, ObligationFulfilledEventSchema e) =>
        state with
        {
            ObligationId = e.AggregateId,
            Status = "Fulfilled"
        };

    public static ObligationReadModel Apply(ObligationReadModel state, ObligationBreachedEventSchema e) =>
        state with
        {
            ObligationId = e.AggregateId,
            Status = "Breached"
        };
}
