namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.4 — sanctioned seam for async finality transitions. Mirrors the
/// dispatch-side <see cref="IOutboundEffectDispatcher"/> pattern: the service
/// validates preconditions, constructs lifecycle events through the T2E
/// factory, and emits them via the canonical event fabric. No caller outside
/// this seam may mutate an <c>OutboundEffectAggregate</c> post-dispatch.
///
/// <para><b>Non-conflation invariant:</b> <see cref="FinalizeAsync"/> transitions
/// only from <c>Acknowledged</c> (or, synchronously-finalized, from
/// <c>Dispatched</c>). <see cref="ReconcileAsync"/> transitions only from
/// <c>ReconciliationRequired</c>. The aggregate refuses invalid source
/// states; the service refuses invalid preconditions before reaching the
/// aggregate.</para>
/// </summary>
public interface IOutboundEffectFinalityService
{
    /// <summary>
    /// Apply a final business outcome after an async callback / poll.
    /// </summary>
    Task FinalizeAsync(
        Guid effectId,
        OutboundFinalityOutcome outcome,
        string evidenceDigest,
        string finalitySource,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Emit <c>OutboundEffectReconciliationRequiredEvent</c> programmatically.
    /// Used by the finality sweeper on ack/finality-deadline expiry and by the
    /// webhook ingress handler on orphan callbacks (when an orphan still
    /// correlates to a known effect id but fails validation).
    /// </summary>
    Task MarkReconciliationRequiredAsync(
        Guid effectId,
        OutboundReconciliationCause cause,
        string diagnosticEvidence,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve a reconciliation-required effect. Strict precondition: the
    /// aggregate MUST be in <c>ReconciliationRequired</c>. Operator actor id
    /// is recorded on the lifecycle event.
    /// </summary>
    Task ReconcileAsync(
        Guid effectId,
        OutboundFinalityOutcome outcome,
        string evidenceDigest,
        string reconcilerActorId,
        CancellationToken cancellationToken = default);
}
