using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.State;

/// <summary>
/// State carried through the transaction lifecycle workflow.
/// Tracks IDs for each stage: instruction → transaction → settlement → ledger.
/// Idempotent: all IDs are pre-assigned at workflow start.
///
/// Phase 4: <see cref="LimitId"/> and <see cref="FxLock"/> are populated by
/// the control-plane steps (CheckLimitStep, FXLockStep) so PostToLedgerStep
/// posts deterministic, control-plane-bound entries. <see cref="FxBaseCurrency"/>
/// is supplied by the intent for cross-currency transactions; null means
/// single-currency and FXLockStep is a no-op.
///
/// SETTLEMENT FINALITY: <see cref="SettlementFinality"/> tracks whether the
/// external settlement provider has confirmed the funds transfer.
/// Compensation logic consults this to determine if a reversal is safe or
/// requires manual reconciliation.
/// </summary>
public sealed class TransactionLifecycleWorkflowState
{
    public Guid InstructionId { get; init; }
    public Guid TransactionId { get; init; }
    public Guid SettlementId { get; init; }
    public Guid LedgerId { get; init; }
    public Guid JournalId { get; init; }
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string InstructionType { get; init; } = string.Empty;
    public string SettlementProvider { get; init; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; init; }
    public string CurrentStep { get; set; } = string.Empty;

    /// <summary>
    /// Phase 4 T4.1 — populated by CheckLimitStep when a per-account limit
    /// is resolved for this transaction. Empty Guid means no limit was
    /// configured (CheckLimitStep ran as a no-op for an open account).
    /// </summary>
    public Guid LimitId { get; set; }

    /// <summary>
    /// Phase 4 T4.4 — when set, indicates the intent requested a cross-currency
    /// transaction with this currency as the base. FXLockStep resolves the
    /// rate from <see cref="FxBaseCurrency"/> → <see cref="Currency"/> and
    /// snapshots it onto <see cref="FxLock"/>.
    /// </summary>
    public string? FxBaseCurrency { get; init; }

    /// <summary>
    /// Phase 4 T4.4 — locked exchange rate snapshot. Null when the
    /// transaction is single-currency. Set by FXLockStep before
    /// PostToLedgerStep so the ledger journal posts at a fixed,
    /// deterministic rate.
    /// </summary>
    public LockedExchangeRate? FxLock { get; set; }

    public SettlementFinalityRecord? SettlementFinality { get; set; }
}

/// <summary>
/// Phase 4 T4.4 — deterministic rate binding for the duration of a single
/// transaction. RateId is the canonical projection key so replay /
/// reconciliation can fetch the exact row that was used.
/// </summary>
public sealed record LockedExchangeRate(
    Guid RateId,
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    DateTimeOffset LockedAt);
