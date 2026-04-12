using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed class VerificationAggregate : AggregateRoot
{
    public VerificationId VerificationId { get; private set; }
    public VerificationSubject Subject { get; private set; }
    public VerificationStatus Status { get; private set; }

    private VerificationAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static VerificationAggregate Initiate(
        VerificationId id,
        VerificationSubject subject)
    {
        var aggregate = new VerificationAggregate();
        aggregate.RaiseDomainEvent(new VerificationInitiatedEvent(id, subject));
        return aggregate;
    }

    // ── Transitions ─────────────────────────────────────────────

    public void Pass()
    {
        if (Status != VerificationStatus.Initiated)
            throw VerificationErrors.InvalidStateTransition(Status, nameof(Pass));

        RaiseDomainEvent(new VerificationPassedEvent(VerificationId));
    }

    public void Fail()
    {
        if (Status != VerificationStatus.Initiated)
            throw VerificationErrors.InvalidStateTransition(Status, nameof(Fail));

        RaiseDomainEvent(new VerificationFailedEvent(VerificationId));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case VerificationInitiatedEvent e:
                VerificationId = e.VerificationId;
                Subject = e.Subject;
                Status = VerificationStatus.Initiated;
                break;

            case VerificationPassedEvent:
                Status = VerificationStatus.Passed;
                break;

            case VerificationFailedEvent:
                Status = VerificationStatus.Failed;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (VerificationId == default)
            throw VerificationErrors.MissingId();

        if (Subject == default)
            throw VerificationErrors.MissingSubject();
    }
}
