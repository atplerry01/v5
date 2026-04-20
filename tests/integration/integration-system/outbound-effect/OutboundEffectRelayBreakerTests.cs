using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Platform.Host.Adapters.OutboundEffects;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.OutboundEffects;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.2 — integration test proving the relay honors per-provider circuit
/// breakers: open-state short-circuits the adapter call AND does NOT consume
/// the retry budget (parent design §6.2).
/// </summary>
public sealed class OutboundEffectRelayBreakerTests
{
    [Fact]
    public async Task BreakerOpen_short_circuits_adapter_and_preserves_attempt_count()
    {
        var clock = new AdvanceableClock(new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.Zero));
        var queue = new InMemoryOutboundEffectQueueStore();
        var adapterCallCount = 0;
        var adapter = new FakeAdapter("test-provider", ct =>
        {
            adapterCallCount++;
            return Task.FromResult<OutboundAdapterResult>(
                new OutboundAdapterResult.Acknowledged(
                    new ProviderOperationIdentity("test-provider", "op-1")));
        });

        // Open-from-the-start breaker. Returns no delay, just throws.
        var openBreaker = new AlwaysOpenBreaker($"{OutboundEffectRelay.BreakerNamePrefix}test-provider", retryAfterSeconds: 10);
        var registry = new FakeBreakerRegistry(openBreaker);

        var options = new OutboundEffectOptions
        {
            ProviderId = "test-provider",
            DispatchTimeoutMs = 5_000,
            TotalBudgetMs = 60_000,
            AckTimeoutMs = 10_000,
            FinalityWindowMs = 60_000,
            MaxAttempts = 5,
        };

        var capturingFabric = new CapturingFabric();

        var relay = new OutboundEffectRelay(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { adapter }),
            new OutboundEffectOptionsRegistry(new[] { options }),
            new OutboundEffectLifecycleEventFactory(new FakePayloadRegistry()),
            capturingFabric,
            clock,
            new FakeRandomProvider(),
            new OutboundEffectsMeter(),
            new OutboundEffectRelayOptions { HostId = "host-a" },
            registry);

        // Seed a queued entry directly.
        var effectId = Guid.Parse("22222222-0000-0000-0000-000000000001");
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = effectId,
            ProviderId = "test-provider",
            EffectType = "test.effect",
            IdempotencyKey = "k-1",
            Status = OutboundEffectQueueStatus.Scheduled,
            AttemptCount = 0,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        var processed = await relay.PollOnceAsync(default);

        Assert.Equal(1, processed);
        Assert.Equal(0, adapterCallCount); // breaker short-circuited
        var entry = await queue.GetAsync(effectId);
        Assert.NotNull(entry);
        Assert.Equal(OutboundEffectQueueStatus.TransientFailed, entry!.Status);
        Assert.Equal(0, entry.AttemptCount); // NOT incremented
        Assert.Contains("breaker_open", entry.LastError);
        // next_attempt_at scheduled at now + 10s retryAfter.
        Assert.InRange(
            entry.NextAttemptAt,
            clock.UtcNow.AddSeconds(9),
            clock.UtcNow.AddSeconds(11));
        // Relay emitted no lifecycle events for breaker-open — attempt never
        // reached the provider.
        Assert.Empty(capturingFabric.ProcessedEvents);
    }

    [Fact]
    public async Task Relay_classifies_adapter_timeout_as_transient_dispatch_failure()
    {
        // The relay bounds each attempt with a linked CTS at
        // options.DispatchTimeoutMs. When that fires, the adapter throws OCE
        // (which it does not classify itself — R3.B.2 delegates to the relay).
        // The relay's outer catch filters on `ct.IsCancellationRequested` to
        // distinguish caller shutdown from attempt timeout.
        var clock = new AdvanceableClock();
        var queue = new InMemoryOutboundEffectQueueStore();
        var adapter = new FakeAdapter("test-provider", async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return (OutboundAdapterResult)new OutboundAdapterResult.Acknowledged(
                new ProviderOperationIdentity("test-provider", "never"));
        });
        var options = new OutboundEffectOptions
        {
            ProviderId = "test-provider",
            DispatchTimeoutMs = 50,
            TotalBudgetMs = 60_000,
            AckTimeoutMs = 10_000,
            FinalityWindowMs = 60_000,
            MaxAttempts = 3,
        };
        var fabric = new CapturingFabric();

        var relay = new OutboundEffectRelay(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { adapter }),
            new OutboundEffectOptionsRegistry(new[] { options }),
            new OutboundEffectLifecycleEventFactory(new FakePayloadRegistry()),
            fabric, clock, new FakeRandomProvider(),
            new OutboundEffectsMeter(),
            new OutboundEffectRelayOptions { HostId = "host-a" });

        var effectId = Guid.Parse("22222222-0000-0000-0000-000000000003");
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = effectId,
            ProviderId = "test-provider",
            EffectType = "test.effect",
            IdempotencyKey = "k-3",
            Status = OutboundEffectQueueStatus.Scheduled,
            AttemptCount = 0,
            MaxAttempts = 3,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        await relay.PollOnceAsync(default);

        var entry = await queue.GetAsync(effectId);
        Assert.Equal(OutboundEffectQueueStatus.TransientFailed, entry!.Status);
        Assert.Equal(1, entry.AttemptCount);
        Assert.Contains("dispatch_timeout", entry.LastError ?? string.Empty);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectDispatchFailedEvent);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectRetryAttemptedEvent);
    }

    [Fact]
    public async Task Happy_path_without_breaker_records_acknowledged_lifecycle()
    {
        var clock = new AdvanceableClock(new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.Zero));
        var queue = new InMemoryOutboundEffectQueueStore();
        var adapter = new FakeAdapter("test-provider", _ =>
            Task.FromResult<OutboundAdapterResult>(
                new OutboundAdapterResult.Acknowledged(
                    new ProviderOperationIdentity("test-provider", "op-happy"))));

        var options = new OutboundEffectOptions
        {
            ProviderId = "test-provider",
            DispatchTimeoutMs = 5_000,
            TotalBudgetMs = 60_000,
            AckTimeoutMs = 10_000,
            FinalityWindowMs = 60_000,
            MaxAttempts = 5,
        };

        var fabric = new CapturingFabric();

        var relay = new OutboundEffectRelay(
            queue,
            new OutboundEffectAdapterRegistry(new IOutboundEffectAdapter[] { adapter }),
            new OutboundEffectOptionsRegistry(new[] { options }),
            new OutboundEffectLifecycleEventFactory(new FakePayloadRegistry()),
            fabric,
            clock,
            new FakeRandomProvider(),
            new OutboundEffectsMeter(),
            new OutboundEffectRelayOptions { HostId = "host-a" },
            breakerRegistry: null);

        var effectId = Guid.Parse("22222222-0000-0000-0000-000000000002");
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = effectId,
            ProviderId = "test-provider",
            EffectType = "test.effect",
            IdempotencyKey = "k-2",
            Status = OutboundEffectQueueStatus.Scheduled,
            AttemptCount = 0,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(1),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });

        var processed = await relay.PollOnceAsync(default);

        Assert.Equal(1, processed);
        var entry = await queue.GetAsync(effectId);
        Assert.Equal(OutboundEffectQueueStatus.Acknowledged, entry!.Status);
        Assert.Equal(1, entry.AttemptCount);
        // Lifecycle: Dispatched + Acknowledged emitted through the fabric.
        Assert.Collection(fabric.ProcessedEvents,
            e => Assert.IsType<OutboundEffectDispatchedEvent>(e),
            e => Assert.IsType<OutboundEffectAcknowledgedEvent>(e));
    }

    // ─────── test doubles ───────

    private sealed class FakeAdapter : IOutboundEffectAdapter
    {
        private readonly Func<CancellationToken, Task<OutboundAdapterResult>> _dispatch;
        public FakeAdapter(string providerId,
            Func<CancellationToken, Task<OutboundAdapterResult>> dispatch)
        {
            ProviderId = providerId;
            _dispatch = dispatch;
        }
        public string ProviderId { get; }
        public OutboundIdempotencyShape IdempotencyShape => OutboundIdempotencyShape.ProviderIdempotent;
        public OutboundFinalityStrategy FinalityStrategy => OutboundFinalityStrategy.ManualOnly;
        public Task<OutboundAdapterResult> DispatchAsync(
            OutboundEffectDispatchContext ctx, CancellationToken ct) => _dispatch(ct);
    }

    private sealed class AlwaysOpenBreaker : ICircuitBreaker
    {
        private readonly int _retryAfterSeconds;
        public AlwaysOpenBreaker(string name, int retryAfterSeconds)
        {
            Name = name;
            _retryAfterSeconds = retryAfterSeconds;
        }
        public string Name { get; }
        public CircuitBreakerState State => CircuitBreakerState.Open;
        public Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default) =>
            throw new CircuitBreakerOpenException(
                Name, _retryAfterSeconds, $"breaker '{Name}' open");
    }

    private sealed class FakeBreakerRegistry : ICircuitBreakerRegistry
    {
        private readonly Dictionary<string, ICircuitBreaker> _breakers;
        public FakeBreakerRegistry(params ICircuitBreaker[] breakers)
        {
            _breakers = breakers.ToDictionary(b => b.Name, StringComparer.Ordinal);
        }
        public ICircuitBreaker Get(string name) => _breakers[name];
        public ICircuitBreaker? TryGet(string name) =>
            _breakers.TryGetValue(name, out var b) ? b : null;
        public IReadOnlyCollection<ICircuitBreaker> GetAll() => _breakers.Values;
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

    private sealed class FakePayloadRegistry : IPayloadTypeRegistry
    {
        public void Register(Type type) { }
        public void Register<T>() { }
        public bool TryGetName(Type type, out string? name) { name = null; return false; }
        public Type Resolve(string typeName) => throw new NotSupportedException();
    }

    private sealed class FakeRandomProvider : IRandomProvider
    {
        public double NextDouble(string seed) => 0.5;
        public int NextInt(string seed, int minInclusive, int maxExclusive) => minInclusive;
        public long NextLong(string seed) => 0L;
    }
}
