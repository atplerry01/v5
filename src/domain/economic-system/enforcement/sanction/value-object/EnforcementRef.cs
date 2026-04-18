namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

/// <summary>
/// Phase 7 T7.10 — the provable link from a sanction to the downstream
/// enforcement aggregate it acts through. Stamped onto
/// <see cref="SanctionActivatedEvent"/> at Activate-time so the
/// sanction's event stream carries the RestrictionId or LockId
/// verbatim; replay reconstructs the coupling without relying on any
/// projection or external join.
///
/// <para>
/// <see cref="EnforcementId"/> is derived deterministically from the
/// sanction id by the handler (e.g. IIdGenerator over
/// "restriction|sanction|{SanctionId:N}" or
/// "lock|sanction|{SanctionId:N}"), so replay and fresh-compute
/// converge on the same value. The event stream is the authoritative
/// record — the derivation is a convenience for first issuance.
/// </para>
///
/// <para>
/// <see cref="Kind"/> must match the parent sanction's
/// <see cref="SanctionType"/> at Activate-time; the aggregate invariant
/// rejects any drift so a Type=Restriction sanction can never carry a
/// Kind=Lock enforcement ref.
/// </para>
/// </summary>
public sealed record EnforcementRef
{
    public SanctionType Kind { get; }
    public Guid EnforcementId { get; }

    public EnforcementRef(SanctionType kind, Guid enforcementId)
    {
        if (enforcementId == Guid.Empty)
            throw new ArgumentException(
                "EnforcementRef requires a non-empty EnforcementId.",
                nameof(enforcementId));

        Kind = kind;
        EnforcementId = enforcementId;
    }

    /// <summary>
    /// Replay-only fallback for V1 event streams (pre-T7.10) that were
    /// persisted before <see cref="SanctionActivatedEvent"/> carried an
    /// <see cref="EnforcementRef"/>. Uses the sanction's own
    /// <see cref="SanctionType"/> and reuses the <see cref="SanctionId"/>
    /// as a degenerate <see cref="EnforcementId"/> — an explicit marker
    /// that the coupling was never recorded, not a forged derivation.
    /// </summary>
    internal static EnforcementRef Legacy(SanctionType kind, Guid sanctionId) =>
        new(kind, sanctionId);
}
