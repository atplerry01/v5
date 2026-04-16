using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Sanction;

namespace Whycespace.Projections.Economic.Enforcement.Sanction.Reducer;

public static class SanctionProjectionReducer
{
    public static SanctionReadModel Apply(SanctionReadModel state, SanctionIssuedEventSchema e) =>
        state with
        {
            SanctionId = e.AggregateId,
            SubjectId = e.SubjectId,
            Type = e.Type,
            Scope = e.Scope,
            Reason = e.Reason,
            Status = "Issued",
            IsActive = false,
            EffectiveAt = e.EffectiveAt,
            ExpiresAt = e.ExpiresAt,
            IssuedAt = e.IssuedAt,
            ActivatedAt = null,
            RevokedAt = null,
            ExpiredAt = null,
            RevocationReason = string.Empty,
            LastUpdatedAt = e.IssuedAt
        };

    public static SanctionReadModel Apply(SanctionReadModel state, SanctionActivatedEventSchema e) =>
        state with
        {
            SanctionId = e.AggregateId,
            Status = "Active",
            IsActive = true,
            ActivatedAt = e.ActivatedAt,
            LastUpdatedAt = e.ActivatedAt
        };

    public static SanctionReadModel Apply(SanctionReadModel state, SanctionExpiredEventSchema e) =>
        state with
        {
            SanctionId = e.AggregateId,
            Status = "Expired",
            IsActive = false,
            ExpiredAt = e.ExpiredAt,
            LastUpdatedAt = e.ExpiredAt
        };

    public static SanctionReadModel Apply(SanctionReadModel state, SanctionRevokedEventSchema e) =>
        state with
        {
            SanctionId = e.AggregateId,
            Status = "Revoked",
            IsActive = false,
            RevokedAt = e.RevokedAt,
            RevocationReason = e.RevocationReason,
            LastUpdatedAt = e.RevokedAt
        };
}
