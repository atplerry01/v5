using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Domain.EconomicSystem.Vault.Metrics;
using Whycespace.Domain.EconomicSystem.Vault.Slice;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

/// <summary>
/// Sole aggregate of the vault context. Owns exactly four VaultSliceEntity
/// instances (Slice1..Slice4) and a VaultMetrics value object that tracks
/// Total / Free / Locked / Invested capital across the slices.
///
/// Doctrine enforced:
///   • Funding: only Slice1 accepts inbound capital.
///   • Investment: Slice1 → Slice2 only.
///   • Revenue (SPV profit): flows into Slice1 only.
///   • Payout debit/credit: Slice1 only.
///   • Metrics invariant: Total = Free + Locked + Invested (VaultMetrics ctor).
///
/// Emits intent-based events. No slice-level credit/debit semantics leak
/// as metrics snapshots — events carry amounts + SliceType only.
/// </summary>
public sealed class VaultAccountAggregate : AggregateRoot
{
    private readonly List<VaultSliceEntity> _slices = new();

    public VaultAccountId VaultAccountId { get; private set; }
    public SubjectId OwnerSubjectId { get; private set; }
    public Currency Currency { get; private set; }
    public VaultAccountStatus Status { get; private set; }
    public VaultMetrics Metrics { get; private set; }
    public IReadOnlyList<VaultSliceEntity> Slices => _slices.AsReadOnly();

    private VaultAccountAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static VaultAccountAggregate Create(
        VaultAccountId vaultAccountId,
        SubjectId ownerSubjectId,
        Currency currency)
    {
        var aggregate = new VaultAccountAggregate();

        aggregate.RaiseDomainEvent(new VaultAccountCreatedEvent(
            vaultAccountId,
            ownerSubjectId,
            currency));

        return aggregate;
    }

    // ── Funding (Slice1 only) ────────────────────────────────────

    public void Fund(Amount amount, Currency currency)
    {
        if (Status == VaultAccountStatus.Closed)
            throw VaultAccountErrors.AccountIsClosed();

        if (currency != Currency)
            throw VaultAccountErrors.CurrencyMismatch(Currency, currency);

        if (amount.Value <= 0m)
            throw VaultAccountErrors.InvalidAmount();

        RaiseDomainEvent(new VaultFundedEvent(
            VaultAccountId.Value.ToString(),
            amount.Value,
            currency.Code));
    }

    // ── Investment (Slice1 → Slice2 only) ────────────────────────

    public void Invest(Amount amount, Currency currency)
    {
        if (Status == VaultAccountStatus.Closed)
            throw VaultAccountErrors.AccountIsClosed();

        if (currency != Currency)
            throw VaultAccountErrors.CurrencyMismatch(Currency, currency);

        if (amount.Value <= 0m)
            throw VaultAccountErrors.InvalidAmount();

        if (amount.Value > Metrics.Free.Value)
            throw VaultAccountErrors.InsufficientFreeCapital(amount, Metrics.Free);

        RaiseDomainEvent(new CapitalAllocatedToSliceEvent(
            VaultAccountId.Value.ToString(),
            amount.Value,
            SliceType.Slice1.ToString(),
            SliceType.Slice2.ToString()));
    }

    // ── Revenue (SPV profit → Slice1) ────────────────────────────

    public void ApplyRevenue(decimal amount, string currency)
    {
        if (Status == VaultAccountStatus.Closed)
            throw VaultAccountErrors.AccountIsClosed();

        if (!string.Equals(currency, Currency.Code, StringComparison.Ordinal))
            throw VaultAccountErrors.CurrencyMismatch(Currency, new Currency(currency));

        if (amount <= 0m)
            throw VaultAccountErrors.InvalidAmount();

        RaiseDomainEvent(new SpvProfitReceivedEvent(
            VaultAccountId.Value.ToString(),
            amount,
            currency,
            SliceType.Slice1));
    }

    // ── Debit (payout — Slice1 only) ─────────────────────────────

    public void DebitSlice(SliceType slice, Amount amount)
    {
        if (Status == VaultAccountStatus.Closed)
            throw VaultAccountErrors.AccountIsClosed();

        if (slice != SliceType.Slice1)
            throw VaultAccountErrors.OnlySlice1Payout(slice);

        if (amount.Value <= 0m)
            throw VaultAccountErrors.InvalidAmount();

        if (amount.Value > Metrics.Free.Value)
            throw VaultAccountErrors.InsufficientFreeCapital(amount, Metrics.Free);

        RaiseDomainEvent(new VaultDebitedEvent(
            VaultAccountId.Value.ToString(),
            amount.Value,
            slice));
    }

    // ── Credit (payout receipt — Slice1 only) ────────────────────

    public void CreditSlice(SliceType slice, Amount amount)
    {
        if (Status == VaultAccountStatus.Closed)
            throw VaultAccountErrors.AccountIsClosed();

        if (slice != SliceType.Slice1)
            throw VaultAccountErrors.OnlySlice1Payout(slice);

        if (amount.Value <= 0m)
            throw VaultAccountErrors.InvalidAmount();

        RaiseDomainEvent(new VaultCreditedEvent(
            VaultAccountId.Value.ToString(),
            amount.Value,
            slice));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case VaultAccountCreatedEvent e:
                VaultAccountId = e.VaultAccountId;
                OwnerSubjectId = e.OwnerSubjectId;
                Currency = e.Currency;
                Status = VaultAccountStatus.Active;
                Metrics = VaultMetrics.Zero();
                _slices.Add(VaultSliceEntity.Create(SliceType.Slice1));
                _slices.Add(VaultSliceEntity.Create(SliceType.Slice2));
                _slices.Add(VaultSliceEntity.Create(SliceType.Slice3));
                _slices.Add(VaultSliceEntity.Create(SliceType.Slice4));
                break;

            case VaultFundedEvent e:
                {
                    var fundedAmount = new Amount(e.Amount);
                    Metrics = Metrics.WithFunding(fundedAmount);
                    FindSliceOrThrow(SliceType.Slice1).Credit(fundedAmount);
                    break;
                }

            case CapitalAllocatedToSliceEvent e:
                {
                    var allocatedAmount = new Amount(e.Amount);
                    Metrics = Metrics.WithInvestment(allocatedAmount);
                    var fromSlice = Enum.Parse<SliceType>(e.FromSlice);
                    FindSliceOrThrow(fromSlice).MoveToInvested(allocatedAmount);
                    break;
                }

            case SpvProfitReceivedEvent e:
                {
                    var revenueAmount = new Amount(e.Amount);
                    Metrics = Metrics.WithFunding(revenueAmount);
                    FindSliceOrThrow(e.Slice).Credit(revenueAmount);
                    break;
                }

            case VaultDebitedEvent e:
                {
                    var debitAmount = new Amount(e.Amount);
                    Metrics = Metrics.WithDebit(debitAmount);
                    FindSliceOrThrow(e.Slice).Debit(debitAmount);
                    break;
                }

            case VaultCreditedEvent e:
                {
                    var creditAmount = new Amount(e.Amount);
                    Metrics = Metrics.WithFunding(creditAmount);
                    FindSliceOrThrow(e.Slice).Credit(creditAmount);
                    break;
                }
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (_slices.Count == 0)
            return; // pre-creation state

        if (_slices.Count != 4)
            throw VaultAccountErrors.SliceCountInvariantViolation(_slices.Count);

        var seen = new HashSet<SliceType>();
        foreach (var slice in _slices)
        {
            if (!seen.Add(slice.SliceType))
                throw VaultAccountErrors.DuplicateSliceTypeInvariantViolation(slice.SliceType);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    private VaultSliceEntity FindSliceOrThrow(SliceType sliceType)
    {
        return _slices.Find(s => s.SliceType == sliceType)
            ?? throw VaultAccountErrors.SliceNotFound(sliceType);
    }
}
