using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Validity;

namespace Whycespace.Projections.Business.Agreement.Commitment.Validity.Reducer;

public static class ValidityProjectionReducer
{
    public static ValidityReadModel Apply(ValidityReadModel state, ValidityCreatedEventSchema e) =>
        state with
        {
            ValidityId = e.AggregateId,
            Status = "Valid",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static ValidityReadModel Apply(ValidityReadModel state, ValidityExpiredEventSchema e) =>
        state with
        {
            ValidityId = e.AggregateId,
            Status = "Expired"
        };

    public static ValidityReadModel Apply(ValidityReadModel state, ValidityInvalidatedEventSchema e) =>
        state with
        {
            ValidityId = e.AggregateId,
            Status = "Invalid"
        };
}
