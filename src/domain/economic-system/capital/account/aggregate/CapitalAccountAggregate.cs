using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CapitalAccountAggregate : AggregateRoot
{
    public AccountId AccountId { get; private set; }
    public OwnerId OwnerId { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public decimal TotalBalance { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal ReservedBalance { get; private set; }
    public CapitalAccountStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }

    private CapitalAccountAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static CapitalAccountAggregate OpenAccount(
        AccountId accountId,
        OwnerId ownerId,
        string currencyCode,
        DateTimeOffset now)
    {
        GuardCurrencyCode(currencyCode);

        var account = new CapitalAccountAggregate
        {
            AccountId = accountId,
            OwnerId = ownerId,
            CurrencyCode = currencyCode.ToUpperInvariant(),
            TotalBalance = 0m,
            AvailableBalance = 0m,
            ReservedBalance = 0m,
            Status = CapitalAccountStatus.Active,
            CreatedAt = now,
            LastUpdatedAt = now
        };

        account.RaiseDomainEvent(new CapitalAccountOpenedEvent(
            accountId,
            ownerId,
            account.CurrencyCode,
            now));

        return account;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Fund(decimal amount, string currencyCode, DateTimeOffset now)
    {
        GuardOperational();
        GuardPositiveAmount(amount);
        GuardCurrencyMatch(currencyCode);

        TotalBalance += amount;
        AvailableBalance += amount;
        LastUpdatedAt = now;

        RaiseDomainEvent(new CapitalFundedEvent(
            AccountId,
            amount,
            CurrencyCode,
            TotalBalance,
            AvailableBalance,
            now));
    }

    public void Allocate(decimal amount, string currencyCode, DateTimeOffset now)
    {
        GuardOperational();
        GuardPositiveAmount(amount);
        GuardCurrencyMatch(currencyCode);

        if (amount > AvailableBalance)
            throw CapitalAccountErrors.InsufficientAvailableBalance(amount, AvailableBalance);

        TotalBalance -= amount;
        AvailableBalance -= amount;
        LastUpdatedAt = now;

        RaiseDomainEvent(new CapitalAllocatedEvent(
            AccountId,
            amount,
            CurrencyCode,
            TotalBalance,
            AvailableBalance,
            now));
    }

    public void Reserve(decimal amount, string currencyCode, DateTimeOffset now)
    {
        GuardOperational();
        GuardPositiveAmount(amount);
        GuardCurrencyMatch(currencyCode);

        if (amount > AvailableBalance)
            throw CapitalAccountErrors.InsufficientAvailableBalance(amount, AvailableBalance);

        AvailableBalance -= amount;
        ReservedBalance += amount;
        LastUpdatedAt = now;

        RaiseDomainEvent(new CapitalReservedEvent(
            AccountId,
            amount,
            CurrencyCode,
            AvailableBalance,
            ReservedBalance,
            now));
    }

    public void ReleaseReservation(decimal amount, string currencyCode, DateTimeOffset now)
    {
        GuardOperational();
        GuardPositiveAmount(amount);
        GuardCurrencyMatch(currencyCode);

        if (amount > ReservedBalance)
            throw CapitalAccountErrors.InsufficientAvailableBalance(amount, ReservedBalance);

        ReservedBalance -= amount;
        AvailableBalance += amount;
        LastUpdatedAt = now;

        RaiseDomainEvent(new ReservationReleasedEvent(
            AccountId,
            amount,
            CurrencyCode,
            AvailableBalance,
            ReservedBalance,
            now));
    }

    public void Freeze(string reason, DateTimeOffset now)
    {
        GuardNotClosed();

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Freeze reason is required.");

        Status = CapitalAccountStatus.Frozen;
        LastUpdatedAt = now;

        RaiseDomainEvent(new CapitalAccountFrozenEvent(
            AccountId,
            reason,
            TotalBalance,
            AvailableBalance,
            ReservedBalance,
            now));
    }

    public void Close(string reason, DateTimeOffset now)
    {
        GuardNotClosed();

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Close reason is required.");

        if (TotalBalance != 0m)
            throw CapitalAccountErrors.CannotCloseWithOutstandingBalance(TotalBalance);

        if (ReservedBalance != 0m)
            throw CapitalAccountErrors.CannotCloseWithReservedFunds(ReservedBalance);

        Status = CapitalAccountStatus.Closed;
        LastUpdatedAt = now;

        RaiseDomainEvent(new CapitalAccountClosedEvent(
            AccountId,
            reason,
            now));
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (TotalBalance < 0m)
            throw CapitalAccountErrors.NegativeTotalBalance(TotalBalance);

        if (AvailableBalance < 0m)
            throw CapitalAccountErrors.NegativeAvailableBalance(AvailableBalance);

        if (ReservedBalance < 0m)
            throw CapitalAccountErrors.NegativeReservedBalance(ReservedBalance);

        if (AvailableBalance + ReservedBalance != TotalBalance)
            throw CapitalAccountErrors.BalanceInvariantViolation(TotalBalance, AvailableBalance, ReservedBalance);
    }

    // ── Guards ───────────────────────────────────────────────────

    private void GuardOperational()
    {
        if (Status == CapitalAccountStatus.Frozen)
            throw CapitalAccountErrors.AccountIsFrozen(AccountId);

        if (Status == CapitalAccountStatus.Closed)
            throw CapitalAccountErrors.AccountIsClosed(AccountId);
    }

    private void GuardNotClosed()
    {
        if (Status == CapitalAccountStatus.Closed)
            throw CapitalAccountErrors.AccountIsClosed(AccountId);
    }

    private static void GuardPositiveAmount(decimal amount)
    {
        if (amount <= 0m)
            throw CapitalAccountErrors.InvalidAmount(amount);
    }

    private static void GuardCurrencyCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw CapitalAccountErrors.InvalidCurrencyCode(code ?? "null");
    }

    private void GuardCurrencyMatch(string currencyCode)
    {
        if (!string.Equals(CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase))
            throw CapitalAccountErrors.CurrencyMismatch(CurrencyCode, currencyCode);
    }
}
