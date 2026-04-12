using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementAggregate : AggregateRoot
{
    public SettlementId SettlementId { get; private set; }
    public Guid JournalId { get; private set; }
    public Guid ObligationId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public SettlementStatus Status { get; private set; }
    public Timestamp InitiatedAt { get; private set; }

    private SettlementAggregate() { }

    public static SettlementAggregate Initiate(
        SettlementId settlementId,
        Guid journalId,
        Guid obligationId,
        Amount amount,
        Currency currency,
        Timestamp initiatedAt)
    {
        Guard.Against(amount.Value <= 0, SettlementErrors.InvalidAmount().Message);
        Guard.Against(journalId == Guid.Empty, SettlementErrors.MissingJournalReference().Message);
        Guard.Against(obligationId == Guid.Empty, SettlementErrors.MissingObligationReference().Message);

        var aggregate = new SettlementAggregate();

        aggregate.RaiseDomainEvent(new SettlementInitiatedEvent(
            settlementId,
            journalId,
            obligationId,
            amount,
            currency,
            initiatedAt));

        return aggregate;
    }

    public void Complete(Timestamp completedAt)
    {
        if (Status == SettlementStatus.Failed)
            throw SettlementErrors.CannotCompleteFailedSettlement();

        if (Status == SettlementStatus.Completed)
            throw SettlementErrors.SettlementAlreadyCompleted();

        if (Status != SettlementStatus.Initiated)
            throw SettlementErrors.SettlementNotInitiated();

        RaiseDomainEvent(new SettlementCompletedEvent(SettlementId, completedAt));
    }

    public void Fail(string reason, Timestamp failedAt)
    {
        if (Status == SettlementStatus.Completed)
            throw SettlementErrors.CannotFailCompletedSettlement();

        if (Status == SettlementStatus.Failed)
            throw SettlementErrors.SettlementAlreadyFailed();

        if (Status != SettlementStatus.Initiated)
            throw SettlementErrors.SettlementNotInitiated();

        RaiseDomainEvent(new SettlementFailedEvent(SettlementId, reason, failedAt));
    }

    protected override void Apply(object @event)
    {
        switch (@event)
        {
            case SettlementInitiatedEvent e:
                SettlementId = e.SettlementId;
                JournalId = e.JournalId;
                ObligationId = e.ObligationId;
                Amount = e.Amount;
                Currency = e.Currency;
                Status = SettlementStatus.Initiated;
                InitiatedAt = e.InitiatedAt;
                break;

            case SettlementCompletedEvent:
                Status = SettlementStatus.Completed;
                break;

            case SettlementFailedEvent:
                Status = SettlementStatus.Failed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Amount.Value <= 0, SettlementErrors.NegativeSettlementAmount().Message);
    }
}
