using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed class LimitAggregate : AggregateRoot
{
    public LimitId LimitId { get; private set; }
    public Guid AccountId { get; private set; }
    public LimitType Type { get; private set; }
    public Amount Threshold { get; private set; }
    public Currency Currency { get; private set; }
    public Amount CurrentUtilization { get; private set; }
    public LimitStatus Status { get; private set; }
    public Timestamp DefinedAt { get; private set; }

    private LimitAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static LimitAggregate Define(
        LimitId limitId,
        Guid accountId,
        LimitType type,
        Amount threshold,
        Currency currency,
        Timestamp definedAt)
    {
        if (threshold.Value <= 0m) throw LimitErrors.InvalidThreshold();
        if (accountId == Guid.Empty) throw LimitErrors.MissingAccountReference();

        var aggregate = new LimitAggregate();
        aggregate.RaiseDomainEvent(new LimitDefinedEvent(
            limitId, accountId, type, threshold, currency, definedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Check(
        Guid transactionId,
        Amount transactionAmount,
        Timestamp checkedAt,
        int expectedVersion = -1)
    {
        if (Status != LimitStatus.Active) throw LimitErrors.LimitNotActive();

        // T1.3 — Aggregate-level concurrency guard. When the caller supplies
        // an expected version (the version they observed when building the
        // command), reject if the aggregate has advanced. Two concurrent
        // checks cannot both pass at the limit boundary: the second command
        // that reaches this handler sees the first's LimitCheckedEvent
        // already applied and fails deterministically.
        if (expectedVersion >= 0 && Version != expectedVersion)
            throw LimitErrors.ConcurrencyConflict(expectedVersion, Version);

        var projectedUtilization = CurrentUtilization.Value + transactionAmount.Value;

        if (projectedUtilization > Threshold.Value)
        {
            RaiseDomainEvent(new LimitExceededEvent(
                LimitId, transactionId, transactionAmount, Threshold, checkedAt));
            throw LimitErrors.LimitExceeded(transactionAmount, Threshold);
        }

        RaiseDomainEvent(new LimitCheckedEvent(
            LimitId, transactionId, transactionAmount,
            new Amount(projectedUtilization), checkedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LimitDefinedEvent e:
                LimitId = e.LimitId;
                AccountId = e.AccountId;
                Type = e.Type;
                Threshold = e.Threshold;
                Currency = e.Currency;
                CurrentUtilization = new Amount(0m);
                Status = LimitStatus.Active;
                DefinedAt = e.DefinedAt;
                break;

            case LimitCheckedEvent e:
                CurrentUtilization = e.CurrentUtilization;
                break;

            case LimitExceededEvent:
                Status = LimitStatus.Exceeded;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Threshold.Value < 0m) throw LimitErrors.NegativeThreshold();
        if (CurrentUtilization.Value < 0m) throw LimitErrors.NegativeUtilization();
    }
}
