namespace Whycespace.Shared.Contracts.Economic.Transaction.Settlement;

/// <summary>
/// Settlement finality model — tracks whether a settlement has reached
/// an externally irrevocable state. Used by the reconciliation layer to
/// detect and flag mismatches between internal state (compensated/failed)
/// and external state (funds committed by provider).
///
/// Finality states:
///   Pending    — settlement initiated, not yet confirmed by provider.
///   Confirmed  — provider confirmed settlement (ExternalReferenceId present).
///   Reversed   — provider reversed a previously-confirmed settlement.
///   Unknown    — provider did not respond within timeout; requires manual check.
///
/// The reconciliation hook uses this to detect the dangerous mismatch:
///   Internal=Failed + External=Confirmed → requires manual reversal.
/// </summary>
public enum SettlementFinalityState
{
    Pending,
    Confirmed,
    Reversed,
    Unknown
}

/// <summary>
/// Tracks the external settlement confirmation status alongside the
/// internal settlement lifecycle. This record is carried on the workflow
/// state so that compensation logic can make informed decisions about
/// whether it is safe to compensate.
/// </summary>
public sealed record SettlementFinalityRecord(
    Guid SettlementId,
    SettlementFinalityState ExternalState,
    string? ExternalReferenceId,
    DateTimeOffset? ConfirmedAt,
    string? MismatchReason)
{
    public static SettlementFinalityRecord Pending(Guid settlementId) =>
        new(settlementId, SettlementFinalityState.Pending, null, null, null);

    public bool RequiresReconciliation =>
        ExternalState == SettlementFinalityState.Confirmed ||
        ExternalState == SettlementFinalityState.Unknown;
}
