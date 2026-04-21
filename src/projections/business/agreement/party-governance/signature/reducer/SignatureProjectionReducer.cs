using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Signature;

namespace Whycespace.Projections.Business.Agreement.PartyGovernance.Signature.Reducer;

public static class SignatureProjectionReducer
{
    public static SignatureReadModel Apply(SignatureReadModel state, SignatureCreatedEventSchema e) =>
        state with
        {
            SignatureId = e.AggregateId,
            Status = "Pending",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static SignatureReadModel Apply(SignatureReadModel state, SignatureSignedEventSchema e) =>
        state with
        {
            SignatureId = e.AggregateId,
            Status = "Signed"
        };

    public static SignatureReadModel Apply(SignatureReadModel state, SignatureRevokedEventSchema e) =>
        state with
        {
            SignatureId = e.AggregateId,
            Status = "Revoked"
        };
}
