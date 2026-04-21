using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Acceptance;

namespace Whycespace.Projections.Business.Agreement.Commitment.Acceptance.Reducer;

public static class AcceptanceProjectionReducer
{
    public static AcceptanceReadModel Apply(AcceptanceReadModel state, AcceptanceCreatedEventSchema e) =>
        state with
        {
            AcceptanceId = e.AggregateId,
            Status = "Pending",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static AcceptanceReadModel Apply(AcceptanceReadModel state, AcceptanceAcceptedEventSchema e) =>
        state with
        {
            AcceptanceId = e.AggregateId,
            Status = "Accepted"
        };

    public static AcceptanceReadModel Apply(AcceptanceReadModel state, AcceptanceRejectedEventSchema e) =>
        state with
        {
            AcceptanceId = e.AggregateId,
            Status = "Rejected"
        };
}
