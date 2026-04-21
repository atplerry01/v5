using Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed class ChargeAggregate : AggregateRoot
{
    public ChargeId ChargeId { get; private set; }
    public TransactionId TransactionId { get; private set; }
    public ChargeType Type { get; private set; }
    public Amount BaseAmount { get; private set; }
    public Amount ChargeAmount { get; private set; }
    public Currency Currency { get; private set; }
    public ChargeStatus Status { get; private set; }
    public Timestamp CalculatedAt { get; private set; }

    private ChargeAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ChargeAggregate Calculate(
        ChargeId chargeId,
        TransactionId transactionId,
        ChargeType type,
        Amount baseAmount,
        Amount chargeAmount,
        Currency currency,
        Timestamp calculatedAt)
    {
        if (baseAmount.Value <= 0m) throw ChargeErrors.InvalidBaseAmount();
        if (chargeAmount.Value < 0m) throw ChargeErrors.InvalidChargeAmount();

        var aggregate = new ChargeAggregate();
        aggregate.RaiseDomainEvent(new ChargeCalculatedEvent(
            chargeId, transactionId.Value, type, baseAmount, chargeAmount, currency, calculatedAt));
        return aggregate;
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed ref.
    public static ChargeAggregate Calculate(
        ChargeId chargeId,
        Guid transactionId,
        ChargeType type,
        Amount baseAmount,
        Amount chargeAmount,
        Currency currency,
        Timestamp calculatedAt)
    {
        if (transactionId == Guid.Empty) throw ChargeErrors.MissingTransactionReference();
        return Calculate(chargeId, new TransactionId(transactionId), type, baseAmount, chargeAmount, currency, calculatedAt);
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void ApplyCharge(Timestamp appliedAt)
    {
        if (Status == ChargeStatus.Applied) throw ChargeErrors.ChargeAlreadyApplied();
        if (Status != ChargeStatus.Calculated) throw ChargeErrors.ChargeNotCalculated();

        RaiseDomainEvent(new ChargeAppliedEvent(ChargeId, TransactionId.Value, ChargeAmount, appliedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ChargeCalculatedEvent e:
                ChargeId = e.ChargeId;
                TransactionId = new TransactionId(e.TransactionId);
                Type = e.Type;
                BaseAmount = e.BaseAmount;
                ChargeAmount = e.ChargeAmount;
                Currency = e.Currency;
                Status = ChargeStatus.Calculated;
                CalculatedAt = e.CalculatedAt;
                break;

            case ChargeAppliedEvent:
                Status = ChargeStatus.Applied;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (ChargeAmount.Value < 0m) throw ChargeErrors.NegativeChargeAmount();
    }
}
