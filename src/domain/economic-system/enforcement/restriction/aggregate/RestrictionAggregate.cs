using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

public sealed class RestrictionAggregate : AggregateRoot
{
    public RestrictionId RestrictionId { get; private set; }
    public SubjectId SubjectId { get; private set; }
    public RestrictionScope Scope { get; private set; }
    public Reason Reason { get; private set; }
    public RestrictionStatus Status { get; private set; }
    public Timestamp AppliedAt { get; private set; }

    private RestrictionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RestrictionAggregate Apply(
        RestrictionId restrictionId,
        SubjectId subjectId,
        RestrictionScope scope,
        Reason reason,
        Timestamp appliedAt)
    {
        if (subjectId.Value == Guid.Empty) throw RestrictionErrors.MissingSubjectReference();
        if (string.IsNullOrWhiteSpace(reason.Value)) throw RestrictionErrors.MissingReason();

        var aggregate = new RestrictionAggregate();
        aggregate.RaiseDomainEvent(new RestrictionAppliedEvent(
            restrictionId, subjectId, scope, reason, appliedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Update(RestrictionScope newScope, Reason newReason, Timestamp updatedAt)
    {
        if (Status != RestrictionStatus.Applied)
            throw RestrictionErrors.CannotUpdateRemovedRestriction();
        if (string.IsNullOrWhiteSpace(newReason.Value))
            throw RestrictionErrors.MissingReason();

        RaiseDomainEvent(new RestrictionUpdatedEvent(
            RestrictionId, SubjectId, newScope, newReason, updatedAt));
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
                break;

            case RestrictionUpdatedEvent e:
                Scope = e.NewScope;
                Reason = e.NewReason;
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
    }
}
