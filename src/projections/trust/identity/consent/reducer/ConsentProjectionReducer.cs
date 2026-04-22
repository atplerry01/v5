using Whycespace.Shared.Contracts.Events.Trust.Identity.Consent;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;

namespace Whycespace.Projections.Trust.Identity.Consent.Reducer;

public static class ConsentProjectionReducer
{
    public static ConsentReadModel Apply(ConsentReadModel state, ConsentGrantedEventSchema e) =>
        state with
        {
            ConsentId = e.AggregateId,
            IdentityReference = e.IdentityReference,
            ConsentScope = e.ConsentScope,
            ConsentPurpose = e.ConsentPurpose,
            Status = "Active",
            GrantedAt = e.GrantedAt,
            LastUpdatedAt = e.GrantedAt
        };

    public static ConsentReadModel Apply(ConsentReadModel state, ConsentRevokedEventSchema e) =>
        state with
        {
            ConsentId = e.AggregateId,
            Status = "Revoked",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static ConsentReadModel Apply(ConsentReadModel state, ConsentExpiredEventSchema e) =>
        state with
        {
            ConsentId = e.AggregateId,
            Status = "Expired",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
}
