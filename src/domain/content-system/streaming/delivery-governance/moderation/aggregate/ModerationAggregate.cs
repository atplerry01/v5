using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public sealed class ModerationAggregate : AggregateRoot
{
    public ModerationId ModerationId { get; private set; }
    public ModerationTargetRef TargetRef { get; private set; }
    public string FlagReason { get; private set; } = string.Empty;
    public ModeratorRef? Moderator { get; private set; }
    public ModerationDecision? Decision { get; private set; }
    public string? Rationale { get; private set; }
    public ModerationStatus Status { get; private set; }
    public Timestamp FlaggedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ModerationAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ModerationAggregate Flag(
        ModerationId moderationId,
        ModerationTargetRef targetRef,
        string flagReason,
        Timestamp flaggedAt)
    {
        if (string.IsNullOrWhiteSpace(flagReason))
            throw ModerationErrors.EmptyFlagReason();

        var aggregate = new ModerationAggregate();
        aggregate.RaiseDomainEvent(new StreamFlaggedEvent(moderationId, targetRef, flagReason.Trim(), flaggedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Assign(ModeratorRef moderator, Timestamp assignedAt)
    {
        if (Status != ModerationStatus.Flagged)
            throw ModerationErrors.CannotAssignUnlessFlagged();

        RaiseDomainEvent(new ModerationAssignedEvent(ModerationId, moderator, assignedAt));
    }

    public void Decide(ModerationDecision decision, string rationale, Timestamp decidedAt)
    {
        if (string.IsNullOrWhiteSpace(rationale))
            throw ModerationErrors.EmptyRationale();

        if (Status != ModerationStatus.InReview)
            throw ModerationErrors.CannotDecideUnlessInReview();

        RaiseDomainEvent(new ModerationDecidedEvent(ModerationId, decision, rationale.Trim(), decidedAt));
    }

    public void Overturn(string rationale, Timestamp overturnedAt)
    {
        if (string.IsNullOrWhiteSpace(rationale))
            throw ModerationErrors.EmptyRationale();

        if (Status != ModerationStatus.Decided)
            throw ModerationErrors.CannotOverturnUnlessDecided();

        RaiseDomainEvent(new ModerationOverturnedEvent(ModerationId, rationale.Trim(), overturnedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StreamFlaggedEvent e:
                ModerationId = e.ModerationId;
                TargetRef = e.TargetRef;
                FlagReason = e.FlagReason;
                Status = ModerationStatus.Flagged;
                FlaggedAt = e.FlaggedAt;
                LastModifiedAt = e.FlaggedAt;
                break;

            case ModerationAssignedEvent e:
                Moderator = e.Moderator;
                Status = ModerationStatus.InReview;
                LastModifiedAt = e.AssignedAt;
                break;

            case ModerationDecidedEvent e:
                Decision = e.Decision;
                Rationale = e.Rationale;
                Status = ModerationStatus.Decided;
                LastModifiedAt = e.DecidedAt;
                break;

            case ModerationOverturnedEvent e:
                Rationale = e.Rationale;
                Status = ModerationStatus.Overturned;
                LastModifiedAt = e.OverturnedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (ModerationId.Value == Guid.Empty)
            throw ModerationErrors.MissingModerationId();

        if (TargetRef.Value == Guid.Empty)
            throw ModerationErrors.MissingTargetRef();
    }
}
