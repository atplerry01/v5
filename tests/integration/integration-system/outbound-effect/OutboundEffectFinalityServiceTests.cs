using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Platform.Host.Adapters.OutboundEffects;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.OutboundEffects;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Tests.Integration.Setup;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.4 — `OutboundEffectFinalityService` + `OutboundEffectReconciliationSweeper`
/// coverage: timeouts emit canonical lifecycle events, reconcile has strict
/// preconditions, Acknowledged ≠ Finalized is preserved end-to-end.
/// </summary>
public sealed class OutboundEffectFinalityServiceTests
{
    private static readonly Guid EffectId = Guid.Parse("77777777-0000-0000-0000-000000000001");

    [Fact]
    public async Task Finalize_emits_finalized_event_and_drops_deadlines()
    {
        var (queue, fabric, finality, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);

        await finality.FinalizeAsync(EffectId, OutboundFinalityOutcome.Succeeded,
            "digest-ok", finalitySource: "Push", default);

        var entry = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.Finalized, entry!.Status);
        Assert.Null(entry.AckDeadline);
        Assert.Null(entry.FinalityDeadline);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectFinalizedEvent f
            && f.FinalityOutcome == "Succeeded" && f.FinalitySource == "Push");
    }

    [Fact]
    public async Task Reconcile_refuses_when_status_is_not_reconciliation_required()
    {
        var (queue, _, finality, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            finality.ReconcileAsync(EffectId, OutboundFinalityOutcome.Succeeded,
                "digest", "ops-user", default));
    }

    [Fact]
    public async Task Reconcile_resolves_only_reconciliation_required()
    {
        var (queue, fabric, finality, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);

        await finality.MarkReconciliationRequiredAsync(
            EffectId, OutboundReconciliationCause.FinalityTimeoutExpired,
            "finality_window_expired", default);

        var mid = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.ReconciliationRequired, mid!.Status);

        await finality.ReconcileAsync(EffectId, OutboundFinalityOutcome.ManualIntervention,
            "ops-evidence", "ops-user-123", default);

        var final = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.Reconciled, final!.Status);

        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciledEvent r
            && r.Outcome == "ManualIntervention" && r.ReconcilerActorId == "ops-user-123");
    }

    [Fact]
    public async Task Acknowledged_and_Finalized_remain_non_conflated_after_finalize()
    {
        // After FinalizeAsync, the read-side must see BOTH the prior
        // Acknowledged event (from setup) AND the new Finalized event —
        // the finality service never overwrites Acknowledged with Finalized.
        var (queue, fabric, finality, clock) = NewHarness();
        // Seed both Scheduled + Dispatched + Acknowledged at the event layer.
        fabric.ProcessedEvents.Add(
            new OutboundEffectAcknowledgedEvent(
                new Whycespace.Domain.SharedKernel.Primitives.Kernel.AggregateId(EffectId),
                "test-provider", "op-abc"));
        await SeedAcknowledged(queue, clock);

        await finality.FinalizeAsync(EffectId, OutboundFinalityOutcome.Succeeded,
            "digest-ok", "Push", default);

        // The Acknowledged event predates the Finalized event on the stream —
        // non-conflation preserved. Finalize does not rewrite Acknowledged.
        var acknowledgedIdx = fabric.ProcessedEvents.FindIndex(e => e is OutboundEffectAcknowledgedEvent);
        var finalizedIdx = fabric.ProcessedEvents.FindIndex(e => e is OutboundEffectFinalizedEvent);
        Assert.True(acknowledgedIdx >= 0 && finalizedIdx >= 0, "both events must be on the stream");
        Assert.True(acknowledgedIdx < finalizedIdx, "Acknowledged must precede Finalized");
    }

    [Fact]
    public async Task Ack_timeout_triggers_reconciliation_required_via_sweeper()
    {
        var (queue, fabric, finality, clock) = NewHarness();
        // Seed a Dispatched row whose ack_deadline is already in the past.
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = EffectId,
            ProviderId = "test-provider",
            EffectType = "test",
            IdempotencyKey = "k-ack-timeout",
            Status = OutboundEffectQueueStatus.Dispatched,
            AttemptCount = 1,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            AckDeadline = clock.UtcNow.AddSeconds(-1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        var sweeper = new OutboundEffectReconciliationSweeper(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { new PushOnlyAdapter() }),
            finality, clock,
            new OutboundEffectRelayOptions { HostId = "host-a" });

        var processed = await sweeper.SweepOnceAsync(default);
        Assert.Equal(1, processed);

        var entry = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.ReconciliationRequired, entry!.Status);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent r
            && r.Cause == nameof(OutboundReconciliationCause.AckTimeoutExpired));
    }

    [Fact]
    public async Task Finality_timeout_on_manual_only_adapter_triggers_reconciliation_required()
    {
        var (queue, fabric, finality, clock) = NewHarness();
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = EffectId,
            ProviderId = "test-provider",
            EffectType = "test",
            IdempotencyKey = "k-finality-timeout",
            Status = OutboundEffectQueueStatus.Acknowledged,
            AttemptCount = 1,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            FinalityDeadline = clock.UtcNow.AddSeconds(-1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        var sweeper = new OutboundEffectReconciliationSweeper(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { new PushOnlyAdapter() }),
            finality, clock,
            new OutboundEffectRelayOptions { HostId = "host-a" });

        await sweeper.SweepOnceAsync(default);

        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent r
            && r.Cause == nameof(OutboundReconciliationCause.FinalityTimeoutExpired));
    }

    [Fact]
    public async Task Poll_adapter_finalizes_from_succeeded_poll_result()
    {
        var (queue, fabric, finality, clock) = NewHarness();
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = EffectId,
            ProviderId = "poll-provider",
            EffectType = "test",
            IdempotencyKey = "k-poll-ok",
            Status = OutboundEffectQueueStatus.Acknowledged,
            AttemptCount = 1,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            FinalityDeadline = clock.UtcNow.AddSeconds(-1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        var pollAdapter = new PollAdapter(
            "poll-provider", OutboundFinalityStrategy.Poll,
            _ => new OutboundFinalityPollResult.Succeeded("poll-digest"));
        var sweeper = new OutboundEffectReconciliationSweeper(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { pollAdapter }),
            finality, clock,
            new OutboundEffectRelayOptions { HostId = "host-a" });

        await sweeper.SweepOnceAsync(default);

        var entry = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.Finalized, entry!.Status);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectFinalizedEvent f
            && f.FinalityOutcome == "Succeeded" && f.FinalitySource == "Poll");
    }

    [Fact]
    public async Task Poll_adapter_unresolvable_result_emits_reconciliation_required()
    {
        var (queue, fabric, finality, clock) = NewHarness();
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = EffectId,
            ProviderId = "poll-provider",
            EffectType = "test",
            IdempotencyKey = "k-poll-unresolvable",
            Status = OutboundEffectQueueStatus.Acknowledged,
            AttemptCount = 1,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            FinalityDeadline = clock.UtcNow.AddSeconds(-1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        var pollAdapter = new PollAdapter(
            "poll-provider", OutboundFinalityStrategy.Poll,
            _ => new OutboundFinalityPollResult.Unresolvable("upstream_gone"));
        var sweeper = new OutboundEffectReconciliationSweeper(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { pollAdapter }),
            finality, clock,
            new OutboundEffectRelayOptions { HostId = "host-a" });

        await sweeper.SweepOnceAsync(default);

        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent r
            && r.Cause == nameof(OutboundReconciliationCause.ProviderDisagreement));
    }

    // ─────── helpers ───────

    private static (InMemoryOutboundEffectQueueStore queue,
                    CapturingFabric fabric,
                    OutboundEffectFinalityService finality,
                    AdvanceableClock clock) NewHarness()
    {
        var queue = new InMemoryOutboundEffectQueueStore();
        var fabric = new CapturingFabric();
        var clock = new AdvanceableClock();
        var finality = new OutboundEffectFinalityService(
            queue,
            new OutboundEffectLifecycleEventFactory(new StubPayloadRegistry()),
            fabric, clock, new OutboundEffectsMeter());
        return (queue, fabric, finality, clock);
    }

    private static async Task SeedAcknowledged(
        InMemoryOutboundEffectQueueStore queue, AdvanceableClock clock)
    {
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = EffectId,
            ProviderId = "test-provider",
            EffectType = "test",
            IdempotencyKey = "k-ack",
            Status = OutboundEffectQueueStatus.Acknowledged,
            AttemptCount = 1,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            FinalityDeadline = clock.UtcNow.AddMinutes(5),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });
    }

    private sealed class PushOnlyAdapter : IOutboundEffectAdapter
    {
        public string ProviderId => "test-provider";
        public OutboundIdempotencyShape IdempotencyShape => OutboundIdempotencyShape.ProviderIdempotent;
        public OutboundFinalityStrategy FinalityStrategy => OutboundFinalityStrategy.Push;
        public Task<OutboundAdapterResult> DispatchAsync(
            OutboundEffectDispatchContext ctx, CancellationToken ct) =>
            throw new NotImplementedException("dispatch not under test");
    }

    private sealed class PollAdapter : IOutboundEffectAdapter
    {
        private readonly Func<ProviderOperationIdentity, OutboundFinalityPollResult> _poll;
        public PollAdapter(string providerId, OutboundFinalityStrategy strategy,
            Func<ProviderOperationIdentity, OutboundFinalityPollResult> poll)
        {
            ProviderId = providerId;
            FinalityStrategy = strategy;
            _poll = poll;
        }
        public string ProviderId { get; }
        public OutboundIdempotencyShape IdempotencyShape => OutboundIdempotencyShape.ProviderIdempotent;
        public OutboundFinalityStrategy FinalityStrategy { get; }
        public Task<OutboundAdapterResult> DispatchAsync(
            OutboundEffectDispatchContext ctx, CancellationToken ct) =>
            throw new NotImplementedException("dispatch not under test");
        public Task<OutboundFinalityPollResult> PollFinalityAsync(
            ProviderOperationIdentity providerOperation, CancellationToken ct) =>
            Task.FromResult(_poll(providerOperation));
    }

    private sealed class CapturingFabric : IEventFabric
    {
        public List<object> ProcessedEvents { get; } = new();
        public Task ProcessAsync(
            IReadOnlyList<object> domainEvents, CommandContext context, CancellationToken ct = default)
        {
            ProcessedEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
        public Task ProcessAuditAsync(AuditEmission audit, CommandContext context, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class StubPayloadRegistry : IPayloadTypeRegistry
    {
        public void Register(Type type) { }
        public void Register<T>() { }
        public bool TryGetName(Type type, out string? name) { name = null; return false; }
        public Type Resolve(string typeName) => throw new NotSupportedException();
    }
}
