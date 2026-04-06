namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed class ObligationAggregate : AggregateRoot
{
    public Guid DebtorId { get; private set; }
    public Guid CreditorId { get; private set; }
    private ObligationAmount _amount = null!;
    private Currency _currency = null!;
    public ObligationStatus Status { get; private set; } = null!;

    private ObligationAggregate() { }

    public static ObligationAggregate Create(Guid id, Guid debtorId, Guid creditorId, ObligationAmount amount, Currency currency)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstDefault(debtorId);
        Guard.AgainstDefault(creditorId);
        Guard.AgainstNull(amount);
        Guard.AgainstNull(currency);
        Guard.AgainstInvalid(amount, a => a.IsPositive, "Obligation amount must be positive.");

        var aggregate = new ObligationAggregate();
        var @event = new ObligationCreatedEvent(id, debtorId, creditorId, amount.Value, currency.Code);
        aggregate.Apply(@event);
        aggregate.RaiseDomainEvent(@event);
        return aggregate;
    }

    public void Activate()
    {
        EnsureInvariant(
            Status == ObligationStatus.Created,
            "ObligationCreated",
            "Only obligations in 'Created' state can be activated.");

        var @event = new ObligationActivatedEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Settle()
    {
        EnsureInvariant(
            Status == ObligationStatus.Active,
            "ObligationActive",
            "Only active obligations can be settled.");

        var @event = new ObligationSettledEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Default(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(
            Status == ObligationStatus.Active,
            "ObligationActive",
            "Only active obligations can be defaulted.");

        var @event = new ObligationDefaultedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    private void Apply(ObligationCreatedEvent @event)
    {
        Id = @event.ObligationId;
        DebtorId = @event.DebtorId;
        CreditorId = @event.CreditorId;
        _amount = new ObligationAmount(@event.Amount);
        _currency = new Currency(@event.CurrencyCode);
        Status = ObligationStatus.Created;
    }

    private void Apply(ObligationActivatedEvent _)
    {
        Status = ObligationStatus.Active;
    }

    private void Apply(ObligationSettledEvent _)
    {
        Status = ObligationStatus.Settled;
    }

    private void Apply(ObligationDefaultedEvent _)
    {
        Status = ObligationStatus.Defaulted;
    }
}
