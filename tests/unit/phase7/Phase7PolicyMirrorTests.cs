using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.Phase7;

/// <summary>
/// Phase 7 B7 — policy ↔ aggregate parity (mirrors B6).
///
/// For every <c>deny_reason</c> emitted by obligation.rego / treasury.rego,
/// the aggregate must reject the same command condition end-of-pipeline.
/// Policy is defense-in-depth: if it is ever bypassed or a caller hits
/// the aggregate via a path that does not invoke OPA, the aggregate's
/// own guard still fires. These tests pin that parity so any future
/// drift in either direction surfaces immediately.
///
/// The mirror table (policy deny_reason → aggregate guard):
///
///   obligation.rego
///     obligation_amount_non_positive   ↔ ObligationErrors.InvalidAmount
///     obligation_counterparty_missing  ↔ ObligationErrors.InvalidCounterparty
///     obligation_already_terminal      ↔ ObligationErrors.ObligationNotPending
///                                        + AlreadyFulfilled / AlreadyCancelled
///                                        + CannotFulfilCancelledObligation
///                                        + CannotCancelFulfilledObligation
///     obligation_type_unknown          — command-DTO-only; no aggregate
///                                        mirror (enum parse at handler
///                                        boundary)
///
///   treasury.rego
///     treasury_non_positive_amount     ↔ TreasuryErrors.InvalidAmount
///     treasury_insufficient_funds      ↔ TreasuryErrors.InsufficientTreasuryFunds
///     treasury_currency_missing        — command-DTO-only; Currency VO
///                                        is a plain wrapper with no
///                                        aggregate mirror
/// </summary>
public sealed class Phase7PolicyMirrorTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 17, 0, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 17, 0, 10, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 17, 0, 20, 0, TimeSpan.Zero));

    private static readonly Currency Usd = new("USD");

    // ══════════════════════════════════════════════════════════════════
    //  OBLIGATION — mirror of obligation.rego deny_reason set
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Obligation_Create_NonPositiveAmount_Rejected_MirrorsPolicy_obligation_amount_non_positive()
    {
        var obligationId = new ObligationId(IdGen.Generate("Oblig:BadAmt:o"));
        var counterparty = IdGen.Generate("Oblig:BadAmt:c");

        // Policy: deny_reason "obligation_amount_non_positive" fires when
        // input.command.amount <= 0. Aggregate mirrors with InvalidAmount.

        Assert.ThrowsAny<Exception>(() =>
            ObligationAggregate.Create(
                obligationId, counterparty,
                ObligationType.Payable, new Amount(0m), Usd, T0));

        Assert.ThrowsAny<Exception>(() =>
            ObligationAggregate.Create(
                obligationId, counterparty,
                ObligationType.Payable, new Amount(-1m), Usd, T0));
    }

    [Fact]
    public void Obligation_Create_MissingCounterparty_Rejected_MirrorsPolicy_obligation_counterparty_missing()
    {
        var obligationId = new ObligationId(IdGen.Generate("Oblig:NoCp:o"));

        // Policy: deny_reason "obligation_counterparty_missing" fires when
        // counterparty_id is empty or Guid.Empty. Aggregate mirrors via
        // InvalidCounterparty.

        Assert.ThrowsAny<Exception>(() =>
            ObligationAggregate.Create(
                obligationId, Guid.Empty,
                ObligationType.Payable, new Amount(100m), Usd, T0));
    }

    [Fact]
    public void Obligation_Fulfil_OnFulfilled_Rejected_MirrorsPolicy_obligation_already_terminal()
    {
        var obligation = NewObligationPending("Oblig:FulfilTerm");
        obligation.Fulfil(IdGen.Generate("settle-1"), T1);

        // Policy denies fulfil when state.status in {Fulfilled, Cancelled}.
        // Aggregate mirrors via ObligationAlreadyFulfilled.
        Assert.ThrowsAny<Exception>(() =>
            obligation.Fulfil(IdGen.Generate("settle-2"), T2));
    }

    [Fact]
    public void Obligation_Fulfil_OnCancelled_Rejected_MirrorsPolicy_obligation_already_terminal()
    {
        var obligation = NewObligationPending("Oblig:FulfilAfterCancel");
        obligation.Cancel("withdrawn", T1);

        Assert.ThrowsAny<Exception>(() =>
            obligation.Fulfil(IdGen.Generate("settle"), T2));
    }

    [Fact]
    public void Obligation_Cancel_OnFulfilled_Rejected_MirrorsPolicy_obligation_already_terminal()
    {
        var obligation = NewObligationPending("Oblig:CancelAfterFulfil");
        obligation.Fulfil(IdGen.Generate("settle"), T1);

        // Aggregate mirror: CannotCancelFulfilledObligation.
        Assert.ThrowsAny<Exception>(() =>
            obligation.Cancel("reversal", T2));
    }

    [Fact]
    public void Obligation_Cancel_OnCancelled_Rejected_MirrorsPolicy_obligation_already_terminal()
    {
        var obligation = NewObligationPending("Oblig:CancelTwice");
        obligation.Cancel("first", T1);

        Assert.ThrowsAny<Exception>(() =>
            obligation.Cancel("second", T2));
    }

    // ══════════════════════════════════════════════════════════════════
    //  TREASURY — mirror of treasury.rego deny_reason set
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Treasury_Allocate_NonPositiveAmount_Rejected_MirrorsPolicy_treasury_non_positive_amount()
    {
        var treasury = NewFundedTreasury("Treas:AllocZero", initialFund: 100m);

        Assert.ThrowsAny<Exception>(() => treasury.AllocateFunds(new Amount(0m)));
        Assert.ThrowsAny<Exception>(() => treasury.AllocateFunds(new Amount(-1m)));
    }

    [Fact]
    public void Treasury_Release_NonPositiveAmount_Rejected_MirrorsPolicy_treasury_non_positive_amount()
    {
        var treasury = NewTreasury("Treas:ReleaseZero");

        Assert.ThrowsAny<Exception>(() => treasury.ReleaseFunds(new Amount(0m)));
        Assert.ThrowsAny<Exception>(() => treasury.ReleaseFunds(new Amount(-1m)));
    }

    [Fact]
    public void Treasury_Allocate_ExceedingBalance_Rejected_MirrorsPolicy_treasury_insufficient_funds()
    {
        var treasury = NewFundedTreasury("Treas:AllocTooMuch", initialFund: 100m);

        // Policy: deny_reason "treasury_insufficient_funds" fires when
        // input.command.amount > input.resource.state.balance. Aggregate
        // mirrors via InsufficientTreasuryFunds.
        var ex = Assert.ThrowsAny<Exception>(() =>
            treasury.AllocateFunds(new Amount(150m)));
        Assert.Contains("Insufficient", ex.Message);
    }

    [Fact]
    public void Treasury_Allocate_ExactBalance_Allowed()
    {
        // Policy mirror: allocate_balance_ok evaluates true when
        // amount == balance. Aggregate mirrors (strict <= check).
        var treasury = NewFundedTreasury("Treas:AllocExact", initialFund: 100m);

        treasury.AllocateFunds(new Amount(100m));

        Assert.Equal(0m, treasury.Balance.Value);
    }

    [Fact]
    public void Treasury_AllocateThenRelease_BalanceTrackedDeterministically()
    {
        // Cross-action consistency: policy approves each individually and
        // the aggregate's resulting balance is deterministic. This is a
        // replay precondition — successive calls produce a predictable
        // trace regardless of interleaving.
        var treasury = NewFundedTreasury("Treas:AllocRelease", initialFund: 500m);

        treasury.AllocateFunds(new Amount(200m));
        Assert.Equal(300m, treasury.Balance.Value);

        treasury.ReleaseFunds(new Amount(50m));
        Assert.Equal(350m, treasury.Balance.Value);

        treasury.AllocateFunds(new Amount(350m));
        Assert.Equal(0m, treasury.Balance.Value);
    }

    // ══════════════════════════════════════════════════════════════════
    //  Notes on unmirrored policy deny reasons
    // ══════════════════════════════════════════════════════════════════
    //
    // `obligation_type_unknown` is not mirrored by any aggregate rejection
    // because ObligationType is a C# enum — the handler's Enum.TryParse
    // at the command boundary is the only enforcement site. The aggregate
    // only ever sees valid enum values.
    //
    // `treasury_currency_missing` is similarly unmirrored: Currency is a
    // thin record struct (`Currency(string Code)`) with no validation, so
    // empty-currency rejection is a command-DTO concern, not an aggregate
    // guard. Both gaps are intentional and documented in B6; the tests
    // above pin every deny reason that HAS an aggregate mirror so no
    // silent drift can occur.

    // ══════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════

    private static ObligationAggregate NewObligationPending(string seed) =>
        ObligationAggregate.Create(
            new ObligationId(IdGen.Generate($"{seed}:o")),
            IdGen.Generate($"{seed}:cp"),
            ObligationType.Payable,
            new Amount(100m),
            Usd,
            T0);

    private static TreasuryAggregate NewTreasury(string seed) =>
        TreasuryAggregate.Create(
            new TreasuryId(IdGen.Generate($"{seed}:t")),
            Usd,
            T0);

    private static TreasuryAggregate NewFundedTreasury(string seed, decimal initialFund)
    {
        var treasury = NewTreasury(seed);
        treasury.ReleaseFunds(new Amount(initialFund));
        return treasury;
    }
}
