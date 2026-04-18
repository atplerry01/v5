using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

/// <summary>
/// Sanction aggregate.
///
/// Phase 7 T7.10/T7.11 — the sanction is the authoritative trigger for
/// enforcement actions. At Activate-time the aggregate stamps an
/// <see cref="EnforcementRef"/> onto its stream binding the sanction
/// to the deterministic RestrictionId or LockId it acts through.
/// Replay reconstructs the coupling exactly — no projection or
/// external join is required to know which restriction/lock belongs to
/// which sanction.
///
/// State transitions (all other pairs are rejected):
///   Issue      →  Issued
///   Activate   :  Issued             → Active   (requires EnforcementRef)
///   Expire     :  Active             → Expired  (terminal — natural)
///   Revoke     :  Issued | Active    → Revoked  (terminal — manual)
///
/// Both terminal transitions populate <see cref="ClearedAt"/> so any
/// downstream reconciliation has one authoritative "enforcement lifted
/// at" timestamp regardless of which path was taken.
///
/// Backward compatibility: V1 <see cref="SanctionActivatedEvent"/>
/// carries <c>Enforcement = null</c>. The Apply handler synthesizes
/// <see cref="EnforcementRef.Legacy"/> on replay so the invariant
/// "Active ↔ Enforcement non-null" stays total over historical streams.
/// </summary>
public sealed class SanctionAggregate : AggregateRoot
{
    public SanctionId SanctionId { get; private set; }
    public SubjectId SubjectId { get; private set; }
    public SanctionType Type { get; private set; }
    public SanctionScope Scope { get; private set; }
    public Reason Reason { get; private set; }
    public EffectivePeriod Period { get; private set; }
    public SanctionStatus Status { get; private set; }
    public Timestamp IssuedAt { get; private set; }

    // Phase 7 T7.10 — cross-aggregate linkage. Non-null once the sanction
    // has been Activated. Kind must match Type; the invariant enforces
    // this so a Restriction-kind sanction can never carry a Lock ref.
    public EnforcementRef? Enforcement { get; private set; }

    // Phase 7 T7.11 — single authoritative "enforcement lifted" timestamp
    // populated on either terminal transition (Expired or Revoked).
    public Timestamp? ClearedAt { get; private set; }

    private SanctionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static SanctionAggregate Issue(
        SanctionId sanctionId,
        SubjectId subjectId,
        SanctionType type,
        SanctionScope scope,
        Reason reason,
        EffectivePeriod period,
        Timestamp issuedAt)
    {
        if (subjectId.Value == Guid.Empty) throw SanctionErrors.MissingSubjectReference();
        if (string.IsNullOrWhiteSpace(reason.Value)) throw SanctionErrors.MissingReason();

        var aggregate = new SanctionAggregate();
        aggregate.RaiseDomainEvent(new SanctionIssuedEvent(
            sanctionId, subjectId, type, scope, reason, period, issuedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    /// <summary>
    /// Phase 7 T7.10 — transitions Issued → Active and stamps the
    /// <see cref="EnforcementRef"/> that binds this sanction to the
    /// downstream RestrictionId or LockId. The ref's <see cref="EnforcementRef.Kind"/>
    /// must match the sanction's <see cref="Type"/>; any mismatch is
    /// rejected so the cross-aggregate coupling cannot drift.
    /// </summary>
    public void Activate(EnforcementRef enforcement, Timestamp activatedAt)
    {
        if (enforcement is null) throw SanctionErrors.EnforcementRefRequired();
        if (enforcement.Kind != Type)
            throw SanctionErrors.EnforcementKindMismatch(Type, enforcement.Kind);

        if (Status == SanctionStatus.Active) throw SanctionErrors.AlreadyActive();
        if (Status != SanctionStatus.Issued)
            throw SanctionErrors.InvalidStateTransition(Status, "activate");

        RaiseDomainEvent(new SanctionActivatedEvent(SanctionId, SubjectId, activatedAt)
        {
            Enforcement = enforcement
        });
    }

    public void Revoke(Reason revocationReason, Timestamp revokedAt)
    {
        if (Status == SanctionStatus.Revoked) throw SanctionErrors.AlreadyRevoked();
        if (Status == SanctionStatus.Expired) throw SanctionErrors.CannotRevokeTerminalSanction(Status);
        if (string.IsNullOrWhiteSpace(revocationReason.Value)) throw SanctionErrors.MissingReason();

        RaiseDomainEvent(new SanctionRevokedEvent(SanctionId, SubjectId, revocationReason, revokedAt));
    }

    public void Expire(Timestamp expiredAt)
    {
        if (Status == SanctionStatus.Expired) throw SanctionErrors.AlreadyExpired();
        if (Status != SanctionStatus.Active) throw SanctionErrors.CannotExpireTerminalSanction(Status);

        RaiseDomainEvent(new SanctionExpiredEvent(SanctionId, SubjectId, expiredAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SanctionIssuedEvent e:
                SanctionId = e.SanctionId;
                SubjectId = e.SubjectId;
                Type = e.Type;
                Scope = e.Scope;
                Reason = e.Reason;
                Period = e.Period;
                Status = SanctionStatus.Issued;
                IssuedAt = e.IssuedAt;
                break;

            case SanctionActivatedEvent e:
                Status = SanctionStatus.Active;
                // V2 events carry the ref verbatim; V1 streams deserialize
                // with Enforcement = null and the aggregate synthesizes a
                // Legacy ref so the invariant stays total.
                Enforcement = e.Enforcement ?? EnforcementRef.Legacy(Type, SanctionId.Value);
                break;

            case SanctionExpiredEvent e:
                Status = SanctionStatus.Expired;
                ClearedAt = e.ExpiredAt;
                break;

            case SanctionRevokedEvent e:
                Status = SanctionStatus.Revoked;
                ClearedAt = e.RevokedAt;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (SanctionId.Value == Guid.Empty) throw SanctionErrors.EmptySanctionId();
        if (SubjectId.Value == Guid.Empty) throw SanctionErrors.OrphanSanction();

        // Phase 7 T7.10 — enforcement linkage required on Active. The
        // terminal states tolerate the ref being null OR non-null; the
        // stream history preserves whatever was recorded at Activate-time.
        if (Status == SanctionStatus.Active && Enforcement is null)
            throw SanctionErrors.EnforcementMissingOnActive();
    }
}
