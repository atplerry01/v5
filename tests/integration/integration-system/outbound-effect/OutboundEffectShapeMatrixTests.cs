using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Platform.Host.Adapters.OutboundEffects;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.OutboundEffects;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.3 — per-shape retry classification matrix. Pins the canonical routing
/// of each (<see cref="OutboundAdapterClassification"/>,
/// <see cref="OutboundIdempotencyShape"/>) pair. Ratified constraint #1 plus
/// <c>R-OUT-EFF-IDEM-05</c> plus <c>R-OUT-EFF-AMBIGUOUS-ATMOSTONCE-01</c>.
///
/// The Ambiguous × {AtMostOnceRequired, CompensatableOnly} cells are the
/// correctness frontier: retrying here would risk a duplicate business
/// outcome the provider cannot collapse, so the relay MUST emit
/// <c>ReconciliationRequired</c> instead of a retry or an exhaustion.
/// </summary>
public sealed class OutboundEffectShapeMatrixTests
{
    [Theory]
    [InlineData(OutboundIdempotencyShape.ProviderIdempotent, OutboundAdapterClassification.Ambiguous, "retry")]
    [InlineData(OutboundIdempotencyShape.NaturalKeyIdempotent, OutboundAdapterClassification.Ambiguous, "retry")]
    [InlineData(OutboundIdempotencyShape.AtMostOnceRequired, OutboundAdapterClassification.Ambiguous, "reconcile")]
    [InlineData(OutboundIdempotencyShape.CompensatableOnly, OutboundAdapterClassification.Ambiguous, "reconcile")]
    [InlineData(OutboundIdempotencyShape.ProviderIdempotent, OutboundAdapterClassification.Transient, "retry")]
    [InlineData(OutboundIdempotencyShape.AtMostOnceRequired, OutboundAdapterClassification.Transient, "retry")]
    [InlineData(OutboundIdempotencyShape.ProviderIdempotent, OutboundAdapterClassification.Terminal, "exhaust")]
    [InlineData(OutboundIdempotencyShape.AtMostOnceRequired, OutboundAdapterClassification.Terminal, "exhaust")]
    public async Task Classification_shape_matrix_lands_on_expected_routing(
        OutboundIdempotencyShape shape,
        OutboundAdapterClassification classification,
        string expected)
    {
        var effectId = Guid.Parse("33333333-0000-0000-0000-000000000001");
        var clock = new AdvanceableClock();
        var queue = new InMemoryOutboundEffectQueueStore();
        var fabric = new CapturingFabric();
        var adapter = new ShapeAdapter("shape-test", shape, classification);

        var options = new OutboundEffectOptions
        {
            ProviderId = "shape-test",
            DispatchTimeoutMs = 5_000,
            TotalBudgetMs = 60_000,
            AckTimeoutMs = 10_000,
            FinalityWindowMs = 60_000,
            MaxAttempts = 5, // well above current attempt so retry branch is reachable
        };

        var relay = new OutboundEffectRelay(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { adapter }),
            new OutboundEffectOptionsRegistry(new[] { options }),
            new OutboundEffectLifecycleEventFactory(new NullPayloadRegistry()),
            fabric, clock, new FixedRandom(),
            new OutboundEffectsMeter(),
            new OutboundEffectRelayOptions { HostId = "host-a" });

        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = effectId,
            ProviderId = "shape-test",
            EffectType = "test.effect",
            IdempotencyKey = $"k-{shape}-{classification}",
            Status = OutboundEffectQueueStatus.Scheduled,
            AttemptCount = 0,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        await relay.PollOnceAsync(default);

        var entry = await queue.GetAsync(effectId);
        Assert.NotNull(entry);

        switch (expected)
        {
            case "retry":
                Assert.Equal(OutboundEffectQueueStatus.TransientFailed, entry!.Status);
                Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectRetryAttemptedEvent);
                Assert.DoesNotContain(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent);
                break;

            case "reconcile":
                Assert.Equal(OutboundEffectQueueStatus.ReconciliationRequired, entry!.Status);
                Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent);
                Assert.DoesNotContain(fabric.ProcessedEvents, e => e is OutboundEffectRetryAttemptedEvent);
                Assert.DoesNotContain(fabric.ProcessedEvents, e => e is OutboundEffectRetryExhaustedEvent);
                break;

            case "exhaust":
                // MaxAttempts is 5 so a single Terminal result exhausts
                // immediately. R3.B.5 additionally emits compensation in the
                // same fabric batch, so the row's terminal status is
                // CompensationRequested (not RetryExhausted — that's the
                // prior state, now a transient step inside the batch).
                Assert.Equal(OutboundEffectQueueStatus.CompensationRequested, entry!.Status);
                Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectRetryExhaustedEvent);
                Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectCompensationRequestedEvent);
                break;

            default:
                throw new InvalidOperationException($"Unknown expected routing: {expected}");
        }
    }

    [Fact]
    public async Task AtMostOnce_Terminal_exhausts_without_reconciliation()
    {
        // Dual of the matrix: Terminal classification on AtMostOnce should
        // NOT flip to reconciliation even though Ambiguous does. Terminal =
        // provider rejected (4xx validation), no ambiguity to reconcile.
        var effectId = Guid.Parse("33333333-0000-0000-0000-000000000099");
        var clock = new AdvanceableClock();
        var queue = new InMemoryOutboundEffectQueueStore();
        var fabric = new CapturingFabric();
        var adapter = new ShapeAdapter("shape-test",
            OutboundIdempotencyShape.AtMostOnceRequired,
            OutboundAdapterClassification.Terminal);
        var options = new OutboundEffectOptions
        {
            ProviderId = "shape-test",
            DispatchTimeoutMs = 5_000,
            TotalBudgetMs = 60_000,
            AckTimeoutMs = 10_000,
            FinalityWindowMs = 60_000,
            MaxAttempts = 5,
        };

        var relay = new OutboundEffectRelay(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { adapter }),
            new OutboundEffectOptionsRegistry(new[] { options }),
            new OutboundEffectLifecycleEventFactory(new NullPayloadRegistry()),
            fabric, clock, new FixedRandom(),
            new OutboundEffectsMeter(),
            new OutboundEffectRelayOptions { HostId = "host-a" });

        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = effectId,
            ProviderId = "shape-test",
            EffectType = "test.effect",
            IdempotencyKey = "k-atmostonce-terminal",
            Status = OutboundEffectQueueStatus.Scheduled,
            AttemptCount = 0,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        await relay.PollOnceAsync(default);

        var entry = await queue.GetAsync(effectId);
        // R3.B.5 — terminal retry-exhaustion now emits compensation in the
        // same batch, so the final status is CompensationRequested. The
        // reconciliation path is still NOT taken (Terminal classification
        // never routes through reconciliation).
        Assert.Equal(OutboundEffectQueueStatus.CompensationRequested, entry!.Status);
        Assert.DoesNotContain(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectRetryExhaustedEvent);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectCompensationRequestedEvent);
    }

    private sealed class ShapeAdapter : IOutboundEffectAdapter
    {
        private readonly OutboundAdapterClassification _classification;
        public ShapeAdapter(string providerId, OutboundIdempotencyShape shape,
            OutboundAdapterClassification classification)
        {
            ProviderId = providerId;
            IdempotencyShape = shape;
            _classification = classification;
        }
        public string ProviderId { get; }
        public OutboundIdempotencyShape IdempotencyShape { get; }
        public OutboundFinalityStrategy FinalityStrategy => OutboundFinalityStrategy.ManualOnly;
        public Task<OutboundAdapterResult> DispatchAsync(
            OutboundEffectDispatchContext ctx, CancellationToken ct) =>
            Task.FromResult<OutboundAdapterResult>(
                new OutboundAdapterResult.DispatchFailedPreAcceptance(_classification, "synthetic"));
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

    private sealed class NullPayloadRegistry : IPayloadTypeRegistry
    {
        public void Register(Type type) { }
        public void Register<T>() { }
        public bool TryGetName(Type type, out string? name) { name = null; return false; }
        public Type Resolve(string typeName) => throw new NotSupportedException();
    }

    private sealed class FixedRandom : IRandomProvider
    {
        public double NextDouble(string seed) => 0.5;
        public int NextInt(string seed, int minInclusive, int maxExclusive) => minInclusive;
        public long NextLong(string seed) => 0L;
    }
}
