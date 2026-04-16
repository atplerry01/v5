using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

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

    public void Activate(Timestamp activatedAt)
    {
        if (Status == SanctionStatus.Active) throw SanctionErrors.AlreadyActive();
        if (Status != SanctionStatus.Issued)
            throw SanctionErrors.InvalidStateTransition(Status, "activate");

        RaiseDomainEvent(new SanctionActivatedEvent(SanctionId, SubjectId, activatedAt));
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

            case SanctionActivatedEvent:
                Status = SanctionStatus.Active;
                break;

            case SanctionExpiredEvent:
                Status = SanctionStatus.Expired;
                break;

            case SanctionRevokedEvent:
                Status = SanctionStatus.Revoked;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (SanctionId.Value == Guid.Empty) throw SanctionErrors.EmptySanctionId();
        if (SubjectId.Value == Guid.Empty) throw SanctionErrors.OrphanSanction();
    }
}
