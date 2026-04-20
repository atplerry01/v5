using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.4 / R-OUT-EFF-TIMEOUT-SWEEP-01 — scans the queue for rows whose ack
/// or finality deadline has elapsed and emits
/// <c>OutboundEffectReconciliationRequiredEvent</c> via
/// <see cref="IOutboundEffectFinalityService"/>. Matches rows where:
/// <list type="bullet">
///   <item><c>status = Dispatched</c> AND <c>ack_deadline &lt;= now</c> →
///         cause <c>AckTimeoutExpired</c>.</item>
///   <item><c>status = Acknowledged</c> AND <c>finality_deadline &lt;= now</c> →
///         cause <c>FinalityTimeoutExpired</c>.</item>
/// </list>
///
/// <para>This type ALSO honors the poll-based finality strategy: for
/// <c>Acknowledged</c> rows belonging to adapters declaring
/// <see cref="OutboundFinalityStrategy.Poll"/> or
/// <see cref="OutboundFinalityStrategy.Hybrid"/>, it calls
/// <see cref="IOutboundEffectAdapter.PollFinalityAsync"/> when the finality
/// deadline has elapsed. A <c>StillPending</c> result extends the deadline;
/// a terminal poll result finalizes; an <c>Unresolvable</c> result emits
/// reconciliation-required.</para>
///
/// <para>Claim semantics identical to the main relay — one host per row via
/// <see cref="IOutboundEffectQueueStore.ClaimExpiredOrPollDueAsync"/>.</para>
/// </summary>
public sealed class OutboundEffectReconciliationSweeper
{
    private readonly IOutboundEffectQueueStore _queueStore;
    private readonly IOutboundEffectAdapterRegistry _adapterRegistry;
    private readonly IOutboundEffectFinalityService _finalityService;
    private readonly IClock _clock;
    private readonly OutboundEffectRelayOptions _options;

    public OutboundEffectReconciliationSweeper(
        IOutboundEffectQueueStore queueStore,
        IOutboundEffectAdapterRegistry adapterRegistry,
        IOutboundEffectFinalityService finalityService,
        IClock clock,
        OutboundEffectRelayOptions options)
    {
        _queueStore = queueStore;
        _adapterRegistry = adapterRegistry;
        _finalityService = finalityService;
        _clock = clock;
        _options = options;
    }

    public async Task<int> SweepOnceAsync(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var claimed = await _queueStore.ClaimExpiredOrPollDueAsync(
            _options.HostId, _options.BatchSize, now, cancellationToken);
        if (claimed.Count == 0) return 0;

        int processed = 0;
        foreach (var entry in claimed)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProcessOneAsync(entry, cancellationToken);
            processed++;
        }
        return processed;
    }

    private async Task ProcessOneAsync(OutboundEffectQueueEntry entry, CancellationToken ct)
    {
        if (entry.Status == OutboundEffectQueueStatus.Dispatched)
        {
            // Ack-deadline expired — provider never acknowledged.
            await _finalityService.MarkReconciliationRequiredAsync(
                entry.EffectId,
                OutboundReconciliationCause.AckTimeoutExpired,
                $"ack_deadline_expired:{entry.AckDeadline:O}",
                ct);
            return;
        }

        if (entry.Status != OutboundEffectQueueStatus.Acknowledged) return;

        // Acknowledged but finality-deadline elapsed. If the adapter supports
        // polling (Poll / Hybrid), attempt one poll before declaring
        // reconciliation-required.
        if (_adapterRegistry.TryGet(entry.ProviderId, out var adapter) && adapter is not null
            && (adapter.FinalityStrategy == OutboundFinalityStrategy.Poll
                || adapter.FinalityStrategy == OutboundFinalityStrategy.Hybrid))
        {
            await PollAndResolveAsync(entry, adapter, ct);
            return;
        }

        await _finalityService.MarkReconciliationRequiredAsync(
            entry.EffectId,
            OutboundReconciliationCause.FinalityTimeoutExpired,
            $"finality_deadline_expired:{entry.FinalityDeadline:O}",
            ct);
    }

    private async Task PollAndResolveAsync(
        OutboundEffectQueueEntry entry,
        IOutboundEffectAdapter adapter,
        CancellationToken ct)
    {
        // ProviderOperationId is carried on the Acknowledged event; for the
        // sweeper we read it from the queue row's last_error slot is NOT
        // appropriate — instead the Acknowledged transition's provider-op-id
        // flows through the projection. R3.B.4 uses the adapter-declared
        // correlation: we pass a stub identity using (ProviderId, EffectId)
        // as the operation id when the queue row has no richer context. In
        // practice, Poll adapters typically correlate by aggregate id anyway.
        var identity = new ProviderOperationIdentity(
            ProviderId: entry.ProviderId,
            ProviderOperationId: entry.EffectId.ToString());

        OutboundFinalityPollResult pollResult;
        try
        {
            pollResult = await adapter.PollFinalityAsync(identity, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            pollResult = new OutboundFinalityPollResult.Transient(
                $"poll_exception:{ex.GetType().Name}:{ex.Message}");
        }

        switch (pollResult)
        {
            case OutboundFinalityPollResult.Succeeded ok:
                await _finalityService.FinalizeAsync(
                    entry.EffectId, OutboundFinalityOutcome.Succeeded,
                    ok.EvidenceDigest, finalitySource: "Poll", ct);
                return;

            case OutboundFinalityPollResult.BusinessFailed bf:
                await _finalityService.FinalizeAsync(
                    entry.EffectId, OutboundFinalityOutcome.BusinessFailed,
                    $"{bf.FailureCode}:{bf.EvidenceDigest}",
                    finalitySource: "Poll", ct);
                return;

            case OutboundFinalityPollResult.PartiallyCompleted pc:
                await _finalityService.FinalizeAsync(
                    entry.EffectId, OutboundFinalityOutcome.PartiallyCompleted,
                    pc.EvidenceDigest, finalitySource: "Poll", ct);
                return;

            case OutboundFinalityPollResult.StillPending:
            case OutboundFinalityPollResult.Transient:
                // Extend the finality deadline by one window; next sweep retries.
                var extend = _clock.UtcNow.AddMilliseconds(entry.FinalityDeadline is null
                    ? 60_000
                    : Math.Max(1_000, (int)(entry.FinalityDeadline.Value - entry.CreatedAt).TotalMilliseconds));
                await _queueStore.UpdateStatusAsync(
                    entry.EffectId,
                    entry.Status,
                    entry.AttemptCount,
                    nextAttemptAt: _clock.UtcNow,
                    ackDeadline: entry.AckDeadline,
                    finalityDeadline: extend,
                    lastError: $"poll_pending:{pollResult.GetType().Name}",
                    updatedAt: _clock.UtcNow,
                    ct);
                return;

            case OutboundFinalityPollResult.Unresolvable ur:
                await _finalityService.MarkReconciliationRequiredAsync(
                    entry.EffectId,
                    OutboundReconciliationCause.ProviderDisagreement,
                    ur.Reason,
                    ct);
                return;

            default:
                throw new InvalidOperationException(
                    $"Unhandled OutboundFinalityPollResult variant: {pollResult.GetType().Name}");
        }
    }
}
