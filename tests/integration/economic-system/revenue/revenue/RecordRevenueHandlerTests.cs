using Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T2E.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Revenue.Revenue;

// D5 — DOM-LIFECYCLE-INIT-IDEMPOTENT-01.b (static-factory sub-clause)
//
// First invocation of RecordRevenueCommand against a deterministic RevenueId
// must succeed and emit RevenueRecordedEvent. Second invocation against the
// SAME RevenueId must throw the typed RevenueErrors.AlreadyRecorded domain
// error — never a storage-layer ConcurrencyConflictException.
public sealed class RecordRevenueHandlerTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public async Task RecordRevenue_FirstInvocation_EmitsRevenueRecordedEvent()
    {
        var revenueId = IdGen.Generate("RecordRevenueHandlerTests:First:revenue");
        var command = new RecordRevenueCommand(
            revenueId, "spv-001", 5_000m, "USD", "invoice-2026-042");
        var ctx = new FakeEngineContext(command, revenueId);
        var handler = new RecordRevenueHandler(new EmptyEventStore());

        await handler.ExecuteAsync(ctx);

        var evt = Assert.IsType<RevenueRecordedEvent>(Assert.Single(ctx.EmittedEvents));
        Assert.Equal(revenueId.ToString(), evt.RevenueId);
        Assert.Equal("spv-001", evt.SpvId);
        Assert.Equal(5_000m, evt.Amount);
    }

    [Fact]
    public async Task RecordRevenue_DuplicateInvocation_ThrowsRevenueAlreadyRecorded_NotConcurrencyConflict()
    {
        var revenueId = IdGen.Generate("RecordRevenueHandlerTests:Duplicate:revenue");
        var command = new RecordRevenueCommand(
            revenueId, "spv-001", 5_000m, "USD", "invoice-2026-042");

        // Pre-seed the event store with one prior RevenueRecordedEvent for the
        // same aggregate. The handler must observe Count > 0 and refuse.
        var priorEvent = new RevenueRecordedEvent(
            revenueId.ToString(), "spv-001", 5_000m, "USD", "invoice-2026-042");
        var preloadedStore = new PreloadedEventStore(revenueId, new object[] { priorEvent });

        var ctx = new FakeEngineContext(command, revenueId);
        var handler = new RecordRevenueHandler(preloadedStore);

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.ExecuteAsync(ctx));
        Assert.Contains("already recorded", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(ctx.EmittedEvents);
    }

    // D5 layered-defense validation: cross-host scenario.
    //
    // Layer 1 (runtime IdempotencyMiddleware) is a per-host in-memory cache
    // keyed on commandId. A duplicate dispatched against a DIFFERENT host
    // (or the same host after a restart that flushes the idempotency cache)
    // bypasses Layer 1 entirely and reaches the engine handler. Layer 2 —
    // the load-then-guard pattern in RecordRevenueHandler — is what protects
    // the aggregate stream in that case. The event store is the only
    // cross-host-shared persistent store we can rely on for this check.
    //
    // This test simulates the cross-host path by invoking the handler
    // directly against an event store that already holds prior events for
    // the same aggregateId — the exact state two hosts would observe after
    // host A committed the first record and host B picked up the duplicate.
    // Layer 2 must return the typed RevenueErrors.AlreadyRecorded without
    // letting a Postgres ConcurrencyConflictException leak from Layer 3.
    [Fact]
    public async Task RecordRevenue_CrossHostScenario_BypassesLayer1_StillReturnsTypedAlreadyRecorded()
    {
        var revenueId = IdGen.Generate("RecordRevenueHandlerTests:CrossHost:revenue");
        var hostBCommand = new RecordRevenueCommand(
            revenueId, "spv-002", 12_500m, "USD", "invoice-2026-CROSS");

        // Host A has already committed one RevenueRecordedEvent at version 0.
        var hostAPriorEvent = new RevenueRecordedEvent(
            revenueId.ToString(), "spv-002", 12_500m, "USD", "invoice-2026-CROSS");
        var sharedEventStore = new PreloadedEventStore(revenueId, new object[] { hostAPriorEvent });

        // Host B has a brand-new in-process IdempotencyMiddleware cache (Layer 1
        // would NOT see hostBCommand as a duplicate — there is no shared cache
        // across hosts). The dispatch reaches the handler.
        var hostBContext = new FakeEngineContext(hostBCommand, revenueId);
        var hostBHandler = new RecordRevenueHandler(sharedEventStore);

        var ex = await Assert.ThrowsAsync<DomainException>(() => hostBHandler.ExecuteAsync(hostBContext));
        Assert.Contains("already recorded", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(hostBContext.EmittedEvents);
    }

    private sealed class FakeEngineContext : IEngineContext
    {
        private readonly List<object> _emitted = new();
        public FakeEngineContext(object command, Guid aggregateId)
        {
            Command = command;
            AggregateId = aggregateId;
        }
        public object Command { get; }
        public Guid AggregateId { get; }
        public string? EnforcementConstraint => null;
        public bool IsSystem => false;
        public IReadOnlyList<object> EmittedEvents => _emitted;
        public Task<object> LoadAggregateAsync(Type aggregateType) =>
            throw new InvalidOperationException("RecordRevenueHandler must not call LoadAggregateAsync.");
        public void EmitEvents(IReadOnlyList<object> events) => _emitted.AddRange(events);
    }

    private sealed class EmptyEventStore : IEventStore
    {
        public Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<object>>(Array.Empty<object>());
        public Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<IEventEnvelope> envelopes, int expectedVersion, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class PreloadedEventStore : IEventStore
    {
        private readonly Guid _id;
        private readonly IReadOnlyList<object> _events;
        public PreloadedEventStore(Guid id, IReadOnlyList<object> events)
        {
            _id = id;
            _events = events;
        }
        public Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken ct = default) =>
            Task.FromResult(aggregateId == _id ? _events : Array.Empty<object>());
        public Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<IEventEnvelope> envelopes, int expectedVersion, CancellationToken ct = default) =>
            Task.CompletedTask;
    }
}
