using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Phase 8 B3 — payout-failure → compensation saga reactor.
///
/// Observes <see cref="PayoutFailedEventSchema"/> envelopes on the payout
/// event topic and deterministically starts <c>PayoutCompensationWorkflow</c>
/// via <see cref="IWorkflowDispatcher"/>. No aggregate mutation, no direct
/// dispatcher bypass — this is a pure event-to-workflow routing layer.
///
/// <para>
/// <b>Two-layer idempotency.</b>
///   (1) Envelope-level claim via <see cref="IIdempotencyStore"/> keyed on
///       <c>payout-failure-compensation:{EventId}</c> short-circuits
///       redelivered Kafka messages before the dispatcher is ever touched.
///       The claim is released on dispatch failure so a genuine retry
///       remains possible.
///   (2) <c>WorkflowDispatcher</c> derives its <c>WorkflowId</c> from
///       <c>(workflowName, payload.ToString())</c> via
///       <c>IIdGenerator</c>, so two invocations with identical payloads
///       collapse to the same workflow id downstream. Combined with the
///       runtime <c>IdempotencyMiddleware</c>, duplicate dispatches
///       dedupe at the command pipeline even if the envelope claim is
///       somehow bypassed.
/// Together these layers produce exactly one workflow per qualifying
/// <c>PayoutFailedEvent</c>, regardless of Kafka at-least-once delivery.
/// </para>
///
/// <para>
/// <b>Correlation.</b> <c>PayoutId</c> flows from the event's
/// <c>AggregateId</c> into <see cref="PayoutCompensationIntent.PayoutId"/>,
/// then into <c>RequestPayoutCompensationCommand.AggregateId</c> when
/// the workflow's first step fires. Replay produces the same correlation
/// chain byte-identically.
/// </para>
///
/// <para>
/// <b>Out-of-order tolerance.</b> The aggregate's own state-machine
/// guards handle misordering — <c>PayoutAggregate.RequestCompensation</c>
/// rejects from any state other than <c>Executed</c> or <c>Failed</c>,
/// so an out-of-order or spurious trigger is caught at the write-side
/// aggregate rather than producing ghost events.
/// </para>
///
/// <para>
/// <b>Intent hydration.</b> <see cref="PayoutCompensationIntent"/>
/// requires fields beyond what the <see cref="PayoutFailedEventSchema"/>
/// carries (Shares, SpvVaultId, ContractId, OriginalJournalId). This
/// handler populates the fields available from the event — PayoutId,
/// DistributionId, Reason — and leaves the remainder at their default
/// values. The workflow's step-level guards (e.g.
/// <c>PostCompensatingLedgerJournalStep</c> requiring a non-empty
/// <c>OriginalJournalId</c>) fail explicitly when a step cannot proceed,
/// surfacing via <c>WorkflowFailedLifecycleEvent</c> for operator
/// attention. Enrichment from deeper projections is follow-up work,
/// not a B3 concern.
/// </para>
/// </summary>
public sealed class PayoutFailureCompensationIntegrationHandler
{
    private const string IdempotencyKeyPrefix = "payout-failure-compensation";

    private static readonly DomainRoute PayoutRoute =
        new("economic", "revenue", "payout");

    private readonly IWorkflowDispatcher _workflowDispatcher;
    private readonly IIdempotencyStore _idempotencyStore;

    public PayoutFailureCompensationIntegrationHandler(
        IWorkflowDispatcher workflowDispatcher,
        IIdempotencyStore idempotencyStore)
    {
        _workflowDispatcher = workflowDispatcher;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.Payload is not PayoutFailedEventSchema failed)
        {
            // Any other payload on the payout events topic is observation-only
            // for this reactor — routing stays narrow to keep correlation
            // and idempotency reasoning local.
            return;
        }

        var idempotencyKey = $"{IdempotencyKeyPrefix}:{envelope.EventId}";
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            var intent = BuildIntent(failed);
            await _workflowDispatcher.StartWorkflowAsync(
                PayoutCompensationWorkflowNames.Compensate,
                intent,
                PayoutRoute);
        }
        catch
        {
            // Release the claim so a genuine retry can proceed. The worker
            // leaves the Kafka offset un-committed on handler exception,
            // so the message will be redelivered after restart and the
            // second attempt will claim cleanly.
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }

    private static PayoutCompensationIntent BuildIntent(PayoutFailedEventSchema failed) =>
        new(
            PayoutId: failed.AggregateId,
            DistributionId: failed.DistributionId,
            ContractId: Guid.Empty,
            SpvId: string.Empty,
            SpvVaultId: Guid.Empty,
            OriginalJournalId: Guid.Empty,
            IdempotencyKey: string.Empty,
            Reason: failed.Reason,
            Shares: Array.Empty<ParticipantPayoutEntry>());
}
