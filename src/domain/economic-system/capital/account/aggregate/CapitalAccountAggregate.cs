using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CapitalAccountAggregate : AggregateRoot
{
    public AccountId AccountId { get; private set; }
    public OwnerId OwnerId { get; private set; }
    public Currency Currency { get; private set; }
    public Amount TotalBalance { get; private set; }
    public Amount AvailableBalance { get; private set; }
    public Amount ReservedBalance { get; private set; }
    public CapitalAccountStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastUpdatedAt { get; private set; }

    public void Open(AccountId accountId, OwnerId ownerId, Currency currency, Timestamp createdAt)
    {
        if (new AlreadyOpenSpecification().IsSatisfiedBy(this))
            throw CapitalAccountErrors.AccountAlreadyOpened();

        RaiseDomainEvent(new CapitalAccountOpenedEvent(accountId, ownerId, currency, createdAt));
    }

    public void Fund(Amount amount, Currency currency)
    {
        if (Status == CapitalAccountStatus.Frozen) throw CapitalAccountErrors.AccountIsFrozen();
        if (Status == CapitalAccountStatus.Closed) throw CapitalAccountErrors.AccountIsClosed();
        if (currency != Currency) throw CapitalAccountErrors.CurrencyMismatch(Currency, currency);
        if (amount.Value <= 0) throw CapitalAccountErrors.InvalidAmount();

        var newTotal = new Amount(TotalBalance.Value + amount.Value);
        var newAvailable = new Amount(AvailableBalance.Value + amount.Value);

        RaiseDomainEvent(new CapitalFundedEvent(AccountId, amount, newTotal, newAvailable));
    }

    public void Allocate(Amount amount, Currency currency)
    {
        if (Status == CapitalAccountStatus.Frozen) throw CapitalAccountErrors.AccountIsFrozen();
        if (Status == CapitalAccountStatus.Closed) throw CapitalAccountErrors.AccountIsClosed();
        if (currency != Currency) throw CapitalAccountErrors.CurrencyMismatch(Currency, currency);
        if (amount.Value <= 0) throw CapitalAccountErrors.InvalidAmount();
        if (amount.Value > AvailableBalance.Value)
            throw CapitalAccountErrors.InsufficientAvailableBalance(amount, AvailableBalance);

        var newAvailable = new Amount(AvailableBalance.Value - amount.Value);

        RaiseDomainEvent(new AccountCapitalAllocatedEvent(AccountId, amount, newAvailable));
    }

    public void Reserve(Amount amount, Currency currency)
    {
        if (Status == CapitalAccountStatus.Frozen) throw CapitalAccountErrors.AccountIsFrozen();
        if (Status == CapitalAccountStatus.Closed) throw CapitalAccountErrors.AccountIsClosed();
        if (currency != Currency) throw CapitalAccountErrors.CurrencyMismatch(Currency, currency);
        if (amount.Value <= 0) throw CapitalAccountErrors.InvalidAmount();
        if (amount.Value > AvailableBalance.Value)
            throw CapitalAccountErrors.InsufficientAvailableBalance(amount, AvailableBalance);

        var newAvailable = new Amount(AvailableBalance.Value - amount.Value);
        var newReserved = new Amount(ReservedBalance.Value + amount.Value);

        RaiseDomainEvent(new AccountCapitalReservedEvent(AccountId, amount, newAvailable, newReserved));
    }

    public void ReleaseReservation(Amount amount, Currency currency)
    {
        if (Status == CapitalAccountStatus.Frozen) throw CapitalAccountErrors.AccountIsFrozen();
        if (Status == CapitalAccountStatus.Closed) throw CapitalAccountErrors.AccountIsClosed();
        if (currency != Currency) throw CapitalAccountErrors.CurrencyMismatch(Currency, currency);
        if (amount.Value <= 0) throw CapitalAccountErrors.InvalidAmount();
        if (amount.Value > ReservedBalance.Value)
            throw CapitalAccountErrors.InsufficientAvailableBalance(amount, ReservedBalance);

        var newAvailable = new Amount(AvailableBalance.Value + amount.Value);
        var newReserved = new Amount(ReservedBalance.Value - amount.Value);

        RaiseDomainEvent(new AccountReservationReleasedEvent(AccountId, amount, newAvailable, newReserved));
    }

    public void Freeze(string reason)
    {
        if (Status == CapitalAccountStatus.Frozen) throw CapitalAccountErrors.AccountIsFrozen();
        if (Status == CapitalAccountStatus.Closed) throw CapitalAccountErrors.AccountIsClosed();

        RaiseDomainEvent(new CapitalAccountFrozenEvent(AccountId, reason));
    }

    public void Close(Timestamp closedAt)
    {
        if (Status == CapitalAccountStatus.Closed) throw CapitalAccountErrors.AccountIsClosed();
        if (TotalBalance.Value != 0) throw CapitalAccountErrors.CannotCloseWithOutstandingBalance();
        if (ReservedBalance.Value != 0) throw CapitalAccountErrors.CannotCloseWithReservedFunds();

        RaiseDomainEvent(new CapitalAccountClosedEvent(AccountId, closedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CapitalAccountOpenedEvent e:
                AccountId = e.AccountId;
                OwnerId = e.OwnerId;
                Currency = e.Currency;
                TotalBalance = new Amount(0);
                AvailableBalance = new Amount(0);
                ReservedBalance = new Amount(0);
                Status = CapitalAccountStatus.Active;
                CreatedAt = e.CreatedAt;
                LastUpdatedAt = e.CreatedAt;
                break;

            case CapitalFundedEvent e:
                TotalBalance = e.NewTotalBalance;
                AvailableBalance = e.NewAvailableBalance;
                break;

            case AccountCapitalAllocatedEvent e:
                TotalBalance = new Amount(TotalBalance.Value - e.AllocatedAmount.Value);
                AvailableBalance = e.NewAvailableBalance;
                break;

            case AccountCapitalReservedEvent e:
                AvailableBalance = e.NewAvailableBalance;
                ReservedBalance = e.NewReservedBalance;
                break;

            case AccountReservationReleasedEvent e:
                AvailableBalance = e.NewAvailableBalance;
                ReservedBalance = e.NewReservedBalance;
                break;

            case CapitalAccountFrozenEvent:
                Status = CapitalAccountStatus.Frozen;
                break;

            case CapitalAccountClosedEvent e:
                Status = CapitalAccountStatus.Closed;
                LastUpdatedAt = e.ClosedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (TotalBalance.Value < 0)
            throw CapitalAccountErrors.NegativeTotalBalance();

        if (AvailableBalance.Value < 0)
            throw CapitalAccountErrors.NegativeAvailableBalance();

        if (ReservedBalance.Value < 0)
            throw CapitalAccountErrors.NegativeReservedBalance();

        if (Status == CapitalAccountStatus.Active &&
            AvailableBalance.Value + ReservedBalance.Value != TotalBalance.Value)
            throw CapitalAccountErrors.BalanceInvariantViolation(TotalBalance, AvailableBalance, ReservedBalance);
    }
}
