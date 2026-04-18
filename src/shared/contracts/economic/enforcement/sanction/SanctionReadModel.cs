namespace Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;

public sealed record SanctionReadModel
{
    public Guid SanctionId { get; init; }
    public Guid SubjectId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset EffectiveAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset IssuedAt { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? RevokedAt { get; init; }
    public DateTimeOffset? ExpiredAt { get; init; }
    public string RevocationReason { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }

    // Phase 7 B5 / T7.10 — authoritative sanction → enforcement linkage.
    // EnforcementKind: "Restriction" | "Lock". EnforcementId is the
    // deterministic RestrictionId or LockId this sanction acts through.
    // Both populated from SanctionActivatedEvent.Enforcement (V2) or
    // synthesized Legacy on V1 replay.
    public string EnforcementKind { get; init; } = string.Empty;
    public Guid? EnforcementId { get; init; }

    // Phase 7 B5 / T7.11 — single authoritative "enforcement lifted"
    // timestamp populated on either terminal transition (Expired or
    // Revoked), mirroring the aggregate's ClearedAt field.
    public DateTimeOffset? ClearedAt { get; init; }
}
