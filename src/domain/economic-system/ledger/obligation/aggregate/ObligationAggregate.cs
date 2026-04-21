using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed class ObligationAggregate : AggregateRoot
{
    public ObligationId ObligationId { get; private set; }
    public CounterpartyRef CounterpartyId { get; private set; }
    public ObligationType Type { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public ObligationStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private ObligationAggregate() { }

    public static ObligationAggregate Create(
        ObligationId obligationId,
        CounterpartyRef counterpartyId,
        ObligationType type,
        Amount amount,
        Currency currency,
        Timestamp createdAt)
    {
        Guard.Against(amount.Value <= 0, ObligationErrors.InvalidAmount().Message);

        var aggregate = new ObligationAggregate();

        aggregate.RaiseDomainEvent(new ObligationCreatedEvent(
            obligationId,
            counterpartyId.Value,
            type,
            amount,
            currency,
            createdAt));

        return aggregate;
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed ref.
    public static ObligationAggregate Create(
        ObligationId obligationId,
        Guid counterpartyId,
        ObligationType type,
        Amount amount,
        Currency currency,
        Timestamp createdAt)
    {
        Guard.Against(counterpartyId == Guid.Empty, ObligationErrors.InvalidCounterparty().Message);
        return Create(obligationId, new CounterpartyRef(counterpartyId), type, amount, currency, createdAt);
    }

    public void Fulfil(Guid settlementId, Timestamp fulfilledAt)
    {
        if (Status == ObligationStatus.Cancelled)
            throw ObligationErrors.CannotFulfilCancelledObligation();

        if (Status == ObligationStatus.Fulfilled)
            throw ObligationErrors.ObligationAlreadyFulfilled();

        if (Status != ObligationStatus.Pending)
            throw ObligationErrors.ObligationNotPending();

        RaiseDomainEvent(new ObligationFulfilledEvent(ObligationId, settlementId, fulfilledAt));
    }

    public void Cancel(string reason, Timestamp cancelledAt)
    {
        if (Status == ObligationStatus.Fulfilled)
            throw ObligationErrors.CannotCancelFulfilledObligation();

        if (Status == ObligationStatus.Cancelled)
            throw ObligationErrors.ObligationAlreadyCancelled();

        if (Status != ObligationStatus.Pending)
            throw ObligationErrors.ObligationNotPending();

        RaiseDomainEvent(new ObligationCancelledEvent(ObligationId, reason, cancelledAt));
    }

    protected override void Apply(object @event)
    {
        switch (@event)
        {
            case ObligationCreatedEvent e:
                ObligationId = e.ObligationId;
                CounterpartyId = new CounterpartyRef(e.CounterpartyId);
                Type = e.Type;
                Amount = e.Amount;
                Currency = e.Currency;
                Status = ObligationStatus.Pending;
                CreatedAt = e.CreatedAt;
                break;

            case ObligationFulfilledEvent:
                Status = ObligationStatus.Fulfilled;
                break;

            case ObligationCancelledEvent:
                Status = ObligationStatus.Cancelled;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Amount.Value <= 0, ObligationErrors.NegativeObligationAmount().Message);
    }
}
