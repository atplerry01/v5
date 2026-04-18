using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

/// <summary>
/// Restriction aggregate.
///
/// Phase 7 T7.6/T7.7 — every Apply carries an
/// <see cref="EnforcementCause"/> that couples the restriction to its
/// triggering aggregate, and the state machine is extended with a
/// reversible <see cref="RestrictionStatus.Suspended"/> pause so
/// in-flight compensation flows can temporarily nullify enforcement
/// without invalidating the underlying <c>Applied</c> record.
///
/// State transitions (all other pairs are rejected):
///   Apply      →  Applied
///   Update     :  Applied                   (Suspended / Removed rejected)
///   Suspend    :  Applied   → Suspended
///   Resume     :  Suspended → Applied
///   Remove     :  Applied | Suspended → Removed  (terminal, irreversible)
///
/// Backward compatibility: V1 streams (pre-T7.6) carry
/// <see cref="RestrictionAppliedEvent"/> instances with <c>Cause = null</c>.
/// The Apply handler synthesizes a <see cref="EnforcementCause.Legacy"/>
/// cause so the post-Apply invariant "Cause non-null on active
/// restriction" stays total across historical replay.
/// </summary>
public sealed class RestrictionAggregate : AggregateRoot
{
    public RestrictionId RestrictionId { get; private set; }
    public SubjectId SubjectId { get; private set; }
    public RestrictionScope Scope { get; private set; }
    public Reason Reason { get; private set; }
    public RestrictionStatus Status { get; private set; }
    public Timestamp AppliedAt { get; private set; }

    // Phase 7 T7.6 — cause-coupling. Non-null on any Applied or
    // Suspended restriction; populated by the Apply event (V2) or a
    // Legacy-synthesized cause on V1 replay.
    public EnforcementCause? Cause { get; private set; }

    private RestrictionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RestrictionAggregate Apply(
        RestrictionId restrictionId,
        SubjectId subjectId,
        RestrictionScope scope,
        Reason reason,
        EnforcementCause cause,
        Timestamp appliedAt)
    {
        if (subjectId.Value == Guid.Empty) throw RestrictionErrors.MissingSubjectReference();
        if (string.IsNullOrWhiteSpace(reason.Value)) throw RestrictionErrors.MissingReason();
        if (cause is null) throw RestrictionErrors.CauseRequired();

        var aggregate = new RestrictionAggregate();
        aggregate.RaiseDomainEvent(new RestrictionAppliedEvent(
            restrictionId, subjectId, scope, reason, appliedAt)
        {
            Cause = cause
        });
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(RestrictionScope newScope, Reason newReason, Timestamp updatedAt)
    {
        if (Status == RestrictionStatus.Removed)
            throw RestrictionErrors.CannotUpdateRemovedRestriction();
        if (Status == RestrictionStatus.Suspended)
            throw RestrictionErrors.CannotUpdateSuspendedRestriction();
        if (string.IsNullOrWhiteSpace(newReason.Value))
            throw RestrictionErrors.MissingReason();

        RaiseDomainEvent(new RestrictionUpdatedEvent(
            RestrictionId, SubjectId, newScope, newReason, updatedAt));
    }

    /// <summary>
    /// Phase 7 T7.7 — temporarily nullify enforcement for a bounded
    /// cause (typically a compensation flow against the same subject).
    /// The original Apply-time <see cref="Cause"/>, <see cref="Scope"/>,
    /// and <see cref="Reason"/> are preserved so <see cref="Resume"/>
    /// restores the pre-suspension state exactly.
    /// </summary>
    public void Suspend(EnforcementCause suspensionCause, Timestamp suspendedAt)
    {
        if (suspensionCause is null) throw RestrictionErrors.CauseRequired();
        if (Status != RestrictionStatus.Applied)
            throw RestrictionErrors.CannotSuspendNonAppliedRestriction(Status);

        RaiseDomainEvent(new RestrictionSuspendedEvent(
            RestrictionId, SubjectId, suspensionCause, suspendedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (Status != RestrictionStatus.Suspended)
            throw RestrictionErrors.CannotResumeNonSuspendedRestriction(Status);

        RaiseDomainEvent(new RestrictionResumedEvent(
            RestrictionId, SubjectId, resumedAt));
    }

    public void Remove(Reason removalReason, Timestamp removedAt)
    {
        if (Status == RestrictionStatus.Removed) throw RestrictionErrors.RestrictionAlreadyRemoved();
        if (string.IsNullOrWhiteSpace(removalReason.Value))
            throw RestrictionErrors.MissingReason();

        RaiseDomainEvent(new RestrictionRemovedEvent(
            RestrictionId, SubjectId, removalReason, removedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RestrictionAppliedEvent e:
                RestrictionId = e.RestrictionId;
                SubjectId = e.SubjectId;
                Scope = e.Scope;
                Reason = e.Reason;
                Status = RestrictionStatus.Applied;
                AppliedAt = e.AppliedAt;
                // V2 events carry Cause; V1 streams replay with null
                // and the invariant tolerates only with a synthesized
                // Legacy cause. This keeps the aggregate state total.
                Cause = e.Cause ?? EnforcementCause.Legacy(e.SubjectId.Value);
                break;

            case RestrictionUpdatedEvent e:
                Scope = e.NewScope;
                Reason = e.NewReason;
                // Cause is NOT overwritten — updates tune scope/reason,
                // not the underlying triggering aggregate.
                break;

            case RestrictionSuspendedEvent:
                Status = RestrictionStatus.Suspended;
                break;

            case RestrictionResumedEvent:
                Status = RestrictionStatus.Applied;
                break;

            case RestrictionRemovedEvent:
                Status = RestrictionStatus.Removed;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (RestrictionId.Value == Guid.Empty) throw RestrictionErrors.EmptyRestrictionId();
        if (SubjectId.Value == Guid.Empty) throw RestrictionErrors.OrphanRestriction();

        // Phase 7 T7.6 — cause-coupling. Active states require a cause;
        // Removed tolerates either (cause becomes historical once terminal).
        if ((Status == RestrictionStatus.Applied || Status == RestrictionStatus.Suspended)
            && Cause is null)
            throw RestrictionErrors.CauseMissingOnActiveRestriction();
    }
}
