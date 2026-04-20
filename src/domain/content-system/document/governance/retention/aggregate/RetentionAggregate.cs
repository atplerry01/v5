using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed class RetentionAggregate : AggregateRoot
{
    public RetentionId RetentionId { get; private set; }
    public RetentionTargetRef TargetRef { get; private set; }
    public RetentionWindow Window { get; private set; }
    public RetentionReason Reason { get; private set; }
    public RetentionStatus Status { get; private set; }
    public Timestamp AppliedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private RetentionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RetentionAggregate Apply(
        RetentionId retentionId,
        RetentionTargetRef targetRef,
        RetentionWindow window,
        RetentionReason reason,
        Timestamp appliedAt)
    {
        var aggregate = new RetentionAggregate();

        aggregate.RaiseDomainEvent(new RetentionAppliedEvent(
            retentionId,
            targetRef,
            window,
            reason,
            appliedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void PlaceHold(RetentionReason reason, Timestamp placedAt)
    {
        if (Status == RetentionStatus.Archived)
            throw RetentionErrors.RetentionArchived();

        if (Status == RetentionStatus.Held)
            throw RetentionErrors.AlreadyHeld();

        if (Status != RetentionStatus.Applied)
            throw RetentionErrors.CannotPlaceHoldOnTerminal();

        RaiseDomainEvent(new RetentionHoldPlacedEvent(RetentionId, reason, placedAt));
    }

    public void Release(Timestamp releasedAt)
    {
        if (Status == RetentionStatus.Archived)
            throw RetentionErrors.RetentionArchived();

        if (Status == RetentionStatus.Released)
            throw RetentionErrors.AlreadyReleased();

        if (Status != RetentionStatus.Applied && Status != RetentionStatus.Held)
            throw RetentionErrors.NotHeld();

        RaiseDomainEvent(new RetentionReleasedEvent(RetentionId, releasedAt));
    }

    public void Expire(Timestamp expiredAt)
    {
        if (Status == RetentionStatus.Archived)
            throw RetentionErrors.RetentionArchived();

        if (Status == RetentionStatus.Expired || Status == RetentionStatus.EligibleForDestruction)
            throw RetentionErrors.AlreadyExpired();

        RaiseDomainEvent(new RetentionExpiredEvent(RetentionId, expiredAt));
    }

    public void MarkEligibleForDestruction(Timestamp markedAt)
    {
        if (Status == RetentionStatus.Archived)
            throw RetentionErrors.RetentionArchived();

        if (Status == RetentionStatus.EligibleForDestruction)
            throw RetentionErrors.AlreadyEligibleForDestruction();

        if (Status != RetentionStatus.Expired)
            throw RetentionErrors.CannotMarkEligibleUnlessExpired();

        RaiseDomainEvent(new RetentionMarkedEligibleForDestructionEvent(RetentionId, markedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == RetentionStatus.Archived)
            throw RetentionErrors.AlreadyArchived();

        RaiseDomainEvent(new RetentionArchivedEvent(RetentionId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RetentionAppliedEvent e:
                RetentionId = e.RetentionId;
                TargetRef = e.TargetRef;
                Window = e.Window;
                Reason = e.Reason;
                Status = RetentionStatus.Applied;
                AppliedAt = e.AppliedAt;
                LastModifiedAt = e.AppliedAt;
                break;

            case RetentionHoldPlacedEvent e:
                Status = RetentionStatus.Held;
                Reason = e.Reason;
                LastModifiedAt = e.PlacedAt;
                break;

            case RetentionReleasedEvent e:
                Status = RetentionStatus.Released;
                LastModifiedAt = e.ReleasedAt;
                break;

            case RetentionExpiredEvent e:
                Status = RetentionStatus.Expired;
                LastModifiedAt = e.ExpiredAt;
                break;

            case RetentionMarkedEligibleForDestructionEvent e:
                Status = RetentionStatus.EligibleForDestruction;
                LastModifiedAt = e.MarkedAt;
                break;

            case RetentionArchivedEvent e:
                Status = RetentionStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (TargetRef.Value == Guid.Empty)
            throw RetentionErrors.OrphanedRetention();
    }
}
