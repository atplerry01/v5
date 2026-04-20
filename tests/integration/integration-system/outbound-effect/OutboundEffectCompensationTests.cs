using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
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
/// R3.B.5 — compensation signaling: atomic emission from canonical problem
/// outcomes, replay-safety, caller-handler consumption, missing-handler
/// evidence.
/// </summary>
public sealed class OutboundEffectCompensationTests
{
    private static readonly Guid EffectId = Guid.Parse("99999999-0000-0000-0000-000000000001");
    private const string ProviderId = "http-webhook";
    private const string EffectType = "payment.settle";

    // ──────────────────── policy ────────────────────

    [Theory]
    [InlineData(OutboundFinalityOutcome.BusinessFailed, OutboundIdempotencyShape.ProviderIdempotent, true)]
    [InlineData(OutboundFinalityOutcome.BusinessFailed, OutboundIdempotencyShape.NaturalKeyIdempotent, true)]
    [InlineData(OutboundFinalityOutcome.BusinessFailed, OutboundIdempotencyShape.AtMostOnceRequired, true)]
    [InlineData(OutboundFinalityOutcome.BusinessFailed, OutboundIdempotencyShape.CompensatableOnly, true)]
    [InlineData(OutboundFinalityOutcome.PartiallyCompleted, OutboundIdempotencyShape.ProviderIdempotent, false)]
    [InlineData(OutboundFinalityOutcome.PartiallyCompleted, OutboundIdempotencyShape.NaturalKeyIdempotent, false)]
    [InlineData(OutboundFinalityOutcome.PartiallyCompleted, OutboundIdempotencyShape.AtMostOnceRequired, true)]
    [InlineData(OutboundFinalityOutcome.PartiallyCompleted, OutboundIdempotencyShape.CompensatableOnly, true)]
    [InlineData(OutboundFinalityOutcome.Succeeded, OutboundIdempotencyShape.ProviderIdempotent, false)]
    [InlineData(OutboundFinalityOutcome.ManualIntervention, OutboundIdempotencyShape.AtMostOnceRequired, false)]
    public void Policy_requires_compensation_matches_design(
        OutboundFinalityOutcome outcome, OutboundIdempotencyShape shape, bool expected)
    {
        Assert.Equal(expected, OutboundEffectCompensationPolicy.RequiresCompensation(outcome, shape));
    }

    // ──────────────────── emission on Finalized(BusinessFailed) ────────────────────

    [Fact]
    public async Task Finalized_BusinessFailed_emits_compensation_atomically_with_finalized()
    {
        var (queue, fabric, dispatcher, handlerInvocations) = await FinalitySetup();
        await SeedAcknowledged(queue, OutboundIdempotencyShape.ProviderIdempotent);

        await (await FinalityService(queue, fabric, dispatcher, adapterShape: OutboundIdempotencyShape.ProviderIdempotent))
            .FinalizeAsync(EffectId, OutboundFinalityOutcome.BusinessFailed, "digest", "Push", default);

        // Both events in the stream; Finalized precedes CompensationRequested.
        var finalizedIdx = fabric.ProcessedEvents.FindIndex(e => e is OutboundEffectFinalizedEvent);
        var compensationIdx = fabric.ProcessedEvents.FindIndex(e => e is OutboundEffectCompensationRequestedEvent);
        Assert.True(finalizedIdx >= 0 && compensationIdx >= 0);
        Assert.True(finalizedIdx < compensationIdx);

        // Queue row lands in CompensationRequested status.
        var entry = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.CompensationRequested, entry!.Status);

        // Handler invoked for this effect type.
        Assert.Single(handlerInvocations);
        Assert.Equal(EffectId, handlerInvocations[0].EffectId);
        Assert.Equal(nameof(OutboundFinalityOutcome.BusinessFailed), handlerInvocations[0].TriggeringOutcome);
    }

    // ──────────────────── emission on Reconciled(BusinessFailed) ────────────────────

    [Fact]
    public async Task Reconciled_BusinessFailed_emits_compensation_atomically_with_reconciled()
    {
        var (queue, fabric, dispatcher, _) = await FinalitySetup();
        await SeedAcknowledged(queue, OutboundIdempotencyShape.ProviderIdempotent);
        var finality = await FinalityService(queue, fabric, dispatcher, OutboundIdempotencyShape.ProviderIdempotent);

        await finality.MarkReconciliationRequiredAsync(
            EffectId, OutboundReconciliationCause.FinalityTimeoutExpired, "expired", default);
        fabric.ProcessedEvents.Clear();

        await finality.ReconcileAsync(
            EffectId, OutboundFinalityOutcome.BusinessFailed, "ops-evidence", "ops-user", default);

        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciledEvent);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectCompensationRequestedEvent);
        var entry = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.CompensationRequested, entry!.Status);
    }

    // ──────────────────── emission on Reconciled(PartiallyCompleted) — shape-gated ────────────────────

    [Fact]
    public async Task Reconciled_PartiallyCompleted_emits_compensation_for_AtMostOnce_shape()
    {
        var (queue, fabric, dispatcher, _) = await FinalitySetup();
        await SeedAcknowledged(queue, OutboundIdempotencyShape.AtMostOnceRequired);
        var finality = await FinalityService(queue, fabric, dispatcher, OutboundIdempotencyShape.AtMostOnceRequired);

        await finality.MarkReconciliationRequiredAsync(
            EffectId, OutboundReconciliationCause.FinalityTimeoutExpired, "expired", default);
        fabric.ProcessedEvents.Clear();

        await finality.ReconcileAsync(
            EffectId, OutboundFinalityOutcome.PartiallyCompleted, "partial-evidence", "ops-user", default);

        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectCompensationRequestedEvent);
    }

    [Fact]
    public async Task Reconciled_PartiallyCompleted_does_NOT_emit_compensation_for_ProviderIdempotent_shape()
    {
        var (queue, fabric, dispatcher, _) = await FinalitySetup();
        await SeedAcknowledged(queue, OutboundIdempotencyShape.ProviderIdempotent);
        var finality = await FinalityService(queue, fabric, dispatcher, OutboundIdempotencyShape.ProviderIdempotent);

        await finality.MarkReconciliationRequiredAsync(
            EffectId, OutboundReconciliationCause.FinalityTimeoutExpired, "expired", default);
        fabric.ProcessedEvents.Clear();

        await finality.ReconcileAsync(
            EffectId, OutboundFinalityOutcome.PartiallyCompleted, "partial", "ops-user", default);

        Assert.DoesNotContain(fabric.ProcessedEvents, e => e is OutboundEffectCompensationRequestedEvent);
        var entry = await queue.GetAsync(EffectId);
        Assert.Equal(OutboundEffectQueueStatus.Reconciled, entry!.Status);
    }

    // ──────────────────── replay non-duplication ────────────────────

    [Fact]
    public void Replay_does_not_duplicate_compensation_emission()
    {
        // The atomic-emission invariant means the aggregate's event stream
        // carries exactly one CompensationRequested per trigger. Replaying
        // the stream through Apply reconstructs the terminal status without
        // causing any re-emission — the aggregate has no post-replay
        // "listener" that could fire again.
        var aggregateId = new AggregateId(EffectId);
        var stream = new object[]
        {
            new OutboundEffectScheduledEvent(aggregateId, ProviderId, EffectType, "k", null, "a", 100, 1000, 500, 10_000, 3),
            new OutboundEffectDispatchedEvent(aggregateId, 1,
                DateTimeOffset.Parse("2026-04-20T12:00:00Z"),
                DateTimeOffset.Parse("2026-04-20T12:00:01Z")),
            new OutboundEffectAcknowledgedEvent(aggregateId, ProviderId, "op-abc"),
            new OutboundEffectFinalizedEvent(aggregateId, "BusinessFailed", "digest",
                DateTimeOffset.Parse("2026-04-20T12:00:02Z"), "Push"),
            new OutboundEffectCompensationRequestedEvent(aggregateId, "BusinessFailed"),
        };

        // Replay twice and compare end state.
        var first = ReplayAggregate(stream);
        var second = ReplayAggregate(stream);

        Assert.Equal(OutboundEffectStatus.CompensationRequested, first.Status);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.FinalityOutcome, second.FinalityOutcome);
    }

    // ──────────────────── repeated-trigger non-duplication ────────────────────

    [Fact]
    public async Task Repeated_finalize_refuses_and_does_not_duplicate_compensation()
    {
        var (queue, fabric, dispatcher, handlerInvocations) = await FinalitySetup();
        await SeedAcknowledged(queue, OutboundIdempotencyShape.ProviderIdempotent);
        var finality = await FinalityService(queue, fabric, dispatcher, OutboundIdempotencyShape.ProviderIdempotent);

        await finality.FinalizeAsync(
            EffectId, OutboundFinalityOutcome.BusinessFailed, "d1", "Push", default);

        Assert.Single(handlerInvocations);

        // Second finalize: aggregate's Apply rejects Finalized-from-CompensationRequested
        // via DomainInvariantViolationException. The compensation event is NOT re-emitted.
        await Assert.ThrowsAnyAsync<Exception>(() =>
            finality.FinalizeAsync(EffectId, OutboundFinalityOutcome.BusinessFailed, "d2", "Push", default));

        // Handler was only invoked once.
        Assert.Single(handlerInvocations);
        var compensationEvents = fabric.ProcessedEvents.OfType<OutboundEffectCompensationRequestedEvent>().ToList();
        Assert.Single(compensationEvents);
    }

    // ──────────────────── example handler + missing handler ────────────────────

    [Fact]
    public async Task Example_handler_receives_signal_and_observes_caller_workflow_trigger()
    {
        // Caller-side handler records incoming compensation signals — this is
        // the "example integration handler" pattern the design calls for. A
        // real caller would dispatch a T1M workflow or enqueue a command here.
        var recorded = new List<OutboundEffectCompensationSignal>();
        var handler = new ExampleCompensationHandler(EffectType, recorded);

        var (queue, fabric, dispatcher, _) = await FinalitySetup(extraHandlers: new[] { handler });
        await SeedAcknowledged(queue, OutboundIdempotencyShape.ProviderIdempotent);
        var finality = await FinalityService(queue, fabric, dispatcher, OutboundIdempotencyShape.ProviderIdempotent);

        await finality.FinalizeAsync(
            EffectId, OutboundFinalityOutcome.BusinessFailed, "digest", "Push", default);

        Assert.Single(recorded);
        Assert.Equal(EffectType, recorded[0].EffectType);
        Assert.Equal(ProviderId, recorded[0].ProviderId);
        Assert.Equal(nameof(OutboundFinalityOutcome.BusinessFailed), recorded[0].TriggeringOutcome);
    }

    [Fact]
    public async Task Missing_handler_still_emits_compensation_event_and_alerts()
    {
        // No handlers registered for this effect type — the dispatcher takes
        // the missing-handler path: the compensation event IS still on the
        // stream (durable), and the dispatcher logs + counts the orphan.
        var (queue, fabric, dispatcher, handlerInvocations) =
            await FinalitySetup(handlerEffectType: "some.other.effect");
        await SeedAcknowledged(queue, OutboundIdempotencyShape.ProviderIdempotent);
        var finality = await FinalityService(queue, fabric, dispatcher, OutboundIdempotencyShape.ProviderIdempotent);

        await finality.FinalizeAsync(
            EffectId, OutboundFinalityOutcome.BusinessFailed, "digest", "Push", default);

        // Compensation event still emitted (durable on the stream).
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectCompensationRequestedEvent);
        // Our recording handler was NOT invoked (it's registered for a different effect type).
        Assert.Empty(handlerInvocations);
    }

    // ──────────────────── helpers ────────────────────

    private static async Task<(InMemoryOutboundEffectQueueStore queue,
                              CapturingFabric fabric,
                              OutboundEffectCompensationDispatcher dispatcher,
                              List<OutboundEffectCompensationSignal> handlerInvocations)>
        FinalitySetup(
            string handlerEffectType = EffectType,
            IOutboundEffectCompensationHandler[]? extraHandlers = null)
    {
        var queue = new InMemoryOutboundEffectQueueStore();
        var fabric = new CapturingFabric();

        var invocations = new List<OutboundEffectCompensationSignal>();
        var primaryHandler = new ExampleCompensationHandler(handlerEffectType, invocations);
        var handlers = new List<IOutboundEffectCompensationHandler> { primaryHandler };
        if (extraHandlers is not null) handlers.AddRange(extraHandlers);

        var registry = new OutboundEffectCompensationHandlerRegistry(handlers);
        var meter = new OutboundEffectsMeter();
        var dispatcher = new OutboundEffectCompensationDispatcher(registry, meter);
        return await Task.FromResult((queue, fabric, dispatcher, invocations));
    }

    private static async Task<OutboundEffectFinalityService> FinalityService(
        InMemoryOutboundEffectQueueStore queue,
        CapturingFabric fabric,
        OutboundEffectCompensationDispatcher dispatcher,
        OutboundIdempotencyShape adapterShape)
    {
        var adapterRegistry = new OutboundEffectAdapterRegistry(
            new IOutboundEffectAdapter[] { new StubAdapter(adapterShape) });
        var clock = new AdvanceableClock();
        var meter = new OutboundEffectsMeter();
        var factory = new OutboundEffectLifecycleEventFactory(new StubPayloadRegistry());
        return await Task.FromResult(new OutboundEffectFinalityService(
            queue, factory, fabric, clock, meter, adapterRegistry, dispatcher));
    }

    private static async Task SeedAcknowledged(
        InMemoryOutboundEffectQueueStore queue,
        OutboundIdempotencyShape shape)
    {
        var clock = new AdvanceableClock();
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = EffectId,
            ProviderId = ProviderId,
            EffectType = EffectType,
            IdempotencyKey = $"k-{shape}",
            Status = OutboundEffectQueueStatus.Acknowledged,
            AttemptCount = 1,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            FinalityDeadline = clock.UtcNow.AddMinutes(10),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });
    }

    private static OutboundEffectAggregate ReplayAggregate(IEnumerable<object> history)
    {
        var ctor = typeof(OutboundEffectAggregate).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null, types: Type.EmptyTypes, modifiers: null);
        var aggregate = (OutboundEffectAggregate)ctor!.Invoke(null);
        aggregate.LoadFromHistory(history);
        return aggregate;
    }

    // ──────────────────── doubles ────────────────────

    private sealed class StubAdapter : IOutboundEffectAdapter
    {
        public StubAdapter(OutboundIdempotencyShape shape) { IdempotencyShape = shape; }
        public string ProviderId => OutboundEffectCompensationTests.ProviderId;
        public OutboundIdempotencyShape IdempotencyShape { get; }
        public OutboundFinalityStrategy FinalityStrategy => OutboundFinalityStrategy.ManualOnly;
        public Task<OutboundAdapterResult> DispatchAsync(
            OutboundEffectDispatchContext ctx, CancellationToken ct) =>
            throw new NotImplementedException("dispatch not under test");
    }

    private sealed class ExampleCompensationHandler : IOutboundEffectCompensationHandler
    {
        private readonly List<OutboundEffectCompensationSignal> _recorded;
        public ExampleCompensationHandler(string effectType, List<OutboundEffectCompensationSignal> recorded)
        {
            EffectType = effectType;
            _recorded = recorded;
        }
        public string EffectType { get; }
        public Task HandleAsync(OutboundEffectCompensationSignal signal, CancellationToken ct)
        {
            _recorded.Add(signal);
            return Task.CompletedTask;
        }
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
