namespace Whycespace.Shared.Contracts.Economic.Transaction.Workflow;

/// <summary>
/// Intent DTO for triggering the transaction lifecycle workflow.
/// Captures the instruction-level data needed to drive the full chain:
/// instruction → transaction → check-limit → settlement → fx-lock → ledger.
///
/// Phase 4: <see cref="FxBaseCurrency"/> is optional. When set, the
/// FXLockStep resolves and binds the FxBaseCurrency → Currency rate before
/// the ledger posts; when null, the lifecycle is single-currency and the
/// FXLockStep is a no-op.
/// </summary>
public sealed record TransactionLifecycleIntent(
    Guid InstructionId,
    Guid TransactionId,
    Guid SettlementId,
    Guid LedgerId,
    Guid JournalId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    string InstructionType,
    string SettlementProvider,
    DateTimeOffset InitiatedAt,
    string? FxBaseCurrency = null);
