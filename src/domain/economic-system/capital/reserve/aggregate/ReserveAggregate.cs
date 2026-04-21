using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed class ReserveAggregate : AggregateRoot
{
    public ReserveId ReserveId { get; private set; }
    public AccountId AccountId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public ReserveStatus Status { get; private set; }
    public Timestamp ReservedAt { get; private set; }
    public Timestamp ExpiresAt { get; private set; }

    private ReserveAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ReserveAggregate Create(
        ReserveId reserveId,
        AccountId accountId,
        Amount amount,
        Currency currency,
        Timestamp reservedAt,
        Timestamp expiresAt)
    {
        if (amount.Value <= 0m)
            throw ReserveErrors.InvalidAmount();

        if (expiresAt.Value <= reservedAt.Value)
            throw ReserveErrors.ExpiryMustBeFuture();

        var aggregate = new ReserveAggregate();

        aggregate.RaiseDomainEvent(new ReserveCreatedEvent(
            reserveId,
            accountId.Value,
            amount,
            currency,
            reservedAt,
            expiresAt));

        return aggregate;
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed ref.
    public static ReserveAggregate Create(
        ReserveId reserveId,
        Guid accountId,
        Amount amount,
        Currency currency,
        Timestamp reservedAt,
        Timestamp expiresAt)
        => Create(reserveId, new AccountId(accountId), amount, currency, reservedAt, expiresAt);

    // ── Behavior ─────────────────────────────────────────────────

    public void Release(Timestamp releasedAt)
    {
        if (Status == ReserveStatus.Expired)
            throw ReserveErrors.CannotReleaseExpiredReservation();

        if (Status == ReserveStatus.Released)
            throw ReserveErrors.ReserveAlreadyReleased();

        if (Status != ReserveStatus.Active)
            throw ReserveErrors.ReservationNotActive();

        RaiseDomainEvent(new ReserveReleasedEvent(
            ReserveId,
            AccountId.Value,
            Amount,
            releasedAt));
    }

    public void Expire(Timestamp expiredAt)
    {
        if (Status == ReserveStatus.Released)
            throw ReserveErrors.CannotExpireReleasedReservation();

        if (Status == ReserveStatus.Expired)
            throw ReserveErrors.ReserveAlreadyExpired();

        if (Status != ReserveStatus.Active)
            throw ReserveErrors.ReservationNotActive();

        RaiseDomainEvent(new ReserveExpiredEvent(
            ReserveId,
            AccountId.Value,
            Amount,
            expiredAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ReserveCreatedEvent e:
                ReserveId = e.ReserveId;
                AccountId = new AccountId(e.AccountId);
                Amount = e.ReservedAmount;
                Currency = e.Currency;
                Status = ReserveStatus.Active;
                ReservedAt = e.ReservedAt;
                ExpiresAt = e.ExpiresAt;
                break;

            case ReserveReleasedEvent:
                Status = ReserveStatus.Released;
                break;

            case ReserveExpiredEvent:
                Status = ReserveStatus.Expired;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Amount.Value < 0m)
            throw ReserveErrors.NegativeReserveAmount();
    }
}
