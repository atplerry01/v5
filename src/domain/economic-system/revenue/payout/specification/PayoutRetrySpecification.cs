namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

/// <summary>
/// Phase 7 T7.5 — idempotent retry rule.
///
/// A re-payout for the same (DistributionId, SpvId) — which resolves to
/// the same deterministic PayoutId — is only permitted when the prior
/// payout aggregate has reached a retryable terminal state:
///
///   * Compensated — the prior attempt was reversed via the compensation
///                   workflow, so a fresh attempt produces no duplicate
///                   financial effect.
///   * Failed      — no ledger side-effect was ever committed, so a
///                   fresh attempt is safe.
///
/// Any other state (Requested, Executed, CompensationRequested) means a
/// prior attempt is in flight or its effects are still present; a retry
/// would double-pay.
///
/// Handlers (T2E) load the prior aggregate, call <see cref="CanRetry"/>,
/// and raise <see cref="PayoutErrors.RetryRequiresCompensatedOrFailed"/>
/// when the rule rejects the attempt.
/// </summary>
public static class PayoutRetrySpecification
{
    public static bool CanRetry(PayoutStatus priorStatus) =>
        priorStatus is PayoutStatus.Compensated or PayoutStatus.Failed;
}
