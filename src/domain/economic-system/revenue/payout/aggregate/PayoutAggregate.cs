using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class PayoutAggregate : AggregateRoot
{
    public PayoutId PayoutId { get; private set; }
    public Guid DistributionId { get; private set; }
    public PayoutStatus Status { get; private set; }
    public Timestamp InitiatedAt { get; private set; }

    private PayoutAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static PayoutAggregate Initiate(
        PayoutId payoutId,
        Guid distributionId,
        Timestamp initiatedAt)
    {
        if (distributionId == Guid.Empty) throw PayoutErrors.MissingDistributionReference();

        var aggregate = new PayoutAggregate();
        aggregate.RaiseDomainEvent(new PayoutInitiatedEvent(
            payoutId, distributionId, initiatedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Complete(Timestamp completedAt)
    {
        if (Status == PayoutStatus.Failed) throw PayoutErrors.CannotCompleteFailedPayout();
        if (Status == PayoutStatus.Completed) throw PayoutErrors.PayoutAlreadyCompleted();
        if (Status != PayoutStatus.Pending) throw PayoutErrors.PayoutNotPending();

        RaiseDomainEvent(new PayoutCompletedEvent(PayoutId, completedAt));
    }

    public void Fail(string reason, Timestamp failedAt)
    {
        if (Status == PayoutStatus.Completed) throw PayoutErrors.CannotFailCompletedPayout();
        if (Status == PayoutStatus.Failed) throw PayoutErrors.PayoutAlreadyFailed();
        if (Status != PayoutStatus.Pending) throw PayoutErrors.PayoutNotPending();

        RaiseDomainEvent(new PayoutFailedEvent(PayoutId, reason, failedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PayoutInitiatedEvent e:
                PayoutId = e.PayoutId;
                DistributionId = e.DistributionId;
                Status = PayoutStatus.Pending;
                InitiatedAt = e.InitiatedAt;
                break;

            case PayoutCompletedEvent:
                Status = PayoutStatus.Completed;
                break;

            case PayoutFailedEvent:
                Status = PayoutStatus.Failed;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (DistributionId == Guid.Empty)
            throw PayoutErrors.MissingDistributionReference();
    }
}
