using System.Collections.Concurrent;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Resilience;

/// <summary>
/// 2.8.22 — Resilience validation for RegistrationActivatedCrossDomainHandler.
///
/// Covers:
///  - idempotency on replay (duplicate event delivery must not double-dispatch)
///  - partial failure containment (dispatch throws → idempotency claim released)
///  - non-activated event type → no-op (wrong payload type guard)
///  - happy path → both domain commands dispatched exactly once
/// </summary>
public sealed class RegistrationActivatedCrossDomainHandlerResilienceTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly TestClock Clock = new();

    // ── Test doubles ──────────────────────────────────────────────────────────

    private sealed class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private readonly ConcurrentDictionary<string, byte> _claims = new();
        public int ClaimAttempts { get; private set; }
        public int ReleaseCount { get; private set; }

        public Task<bool> TryClaimAsync(string key, CancellationToken ct = default)
        {
            ClaimAttempts++;
            return Task.FromResult(_claims.TryAdd(key, 0));
        }

        public Task ReleaseAsync(string key, CancellationToken ct = default)
        {
            ReleaseCount++;
            _claims.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        [Obsolete] public Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
            Task.FromResult(_claims.ContainsKey(key));
        [Obsolete] public Task MarkAsync(string key, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class RecordingDispatcher : ISystemIntentDispatcher
    {
        public List<(object Command, DomainRoute Route)> Dispatches { get; } = new();
        private readonly Exception? _throwOn;

        public RecordingDispatcher(Exception? throwOn = null) => _throwOn = throwOn;

        public Task<CommandResult> DispatchAsync(object command, DomainRoute route, CancellationToken ct = default)
            => Task.FromResult(CommandResult.Success(Array.Empty<object>()));

        public Task<CommandResult> DispatchSystemAsync(object command, DomainRoute route, CancellationToken ct = default)
        {
            if (_throwOn is not null) throw _throwOn;
            Dispatches.Add((command, route));
            return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
        }
    }

    private sealed class StubEnvelope : IEventEnvelope
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid AggregateId { get; init; }
        public Guid CorrelationId { get; init; } = Guid.Empty;
        public Guid CausationId { get; init; } = Guid.Empty;
        public string EventType { get; init; } = "RegistrationActivatedEvent";
        public string EventName { get; init; } = "RegistrationActivatedEvent";
        public string EventVersion { get; init; } = "1.0";
        public string SchemaHash { get; init; } = string.Empty;
        public object Payload { get; init; } = null!;
        public string ExecutionHash { get; init; } = string.Empty;
        public string PolicyHash { get; init; } = string.Empty;
        public string? PolicyVersion { get; init; }
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
        public int SequenceNumber { get; init; }
        public string Classification { get; init; } = "trust";
        public string Context { get; init; } = "identity";
        public string Domain { get; init; } = "registry";
    }

    private static RegistrationActivatedCrossDomainHandler BuildHandler(
        ISystemIntentDispatcher dispatcher,
        IIdempotencyStore idempotencyStore) =>
        new(dispatcher, IdGen, Clock, idempotencyStore);

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_HappyPath_DispatchesBothDomainCommands()
    {
        var registryId = IdGen.Generate("resilience:handler:happy");
        var dispatcher = new RecordingDispatcher();
        var store = new InMemoryIdempotencyStore();
        var handler = BuildHandler(dispatcher, store);

        var envelope = new StubEnvelope
        {
            EventId = IdGen.Generate("resilience:handler:happy:event"),
            AggregateId = registryId,
            Payload = new RegistrationActivatedEventSchema(registryId)
        };

        await handler.HandleAsync(envelope);

        Assert.Equal(2, dispatcher.Dispatches.Count);
        Assert.Equal("structural", dispatcher.Dispatches[0].Route.Classification);
        Assert.Equal("economic", dispatcher.Dispatches[1].Route.Classification);
    }

    [Fact]
    public async Task HandleAsync_DuplicateEventId_SkipsDispatch()
    {
        var registryId = IdGen.Generate("resilience:handler:duplicate");
        var eventId = IdGen.Generate("resilience:handler:duplicate:event");
        var dispatcher = new RecordingDispatcher();
        var store = new InMemoryIdempotencyStore();
        var handler = BuildHandler(dispatcher, store);

        var envelope = new StubEnvelope
        {
            EventId = eventId,
            AggregateId = registryId,
            Payload = new RegistrationActivatedEventSchema(registryId)
        };

        await handler.HandleAsync(envelope);
        await handler.HandleAsync(envelope);

        Assert.Equal(2, dispatcher.Dispatches.Count);
        Assert.Equal(2, store.ClaimAttempts);
    }

    [Fact]
    public async Task HandleAsync_WhenDispatchThrows_ReleasesIdempotencyClaim()
    {
        var registryId = IdGen.Generate("resilience:handler:throw");
        var dispatcher = new RecordingDispatcher(throwOn: new InvalidOperationException("structural BC unavailable"));
        var store = new InMemoryIdempotencyStore();
        var handler = BuildHandler(dispatcher, store);

        var envelope = new StubEnvelope
        {
            EventId = IdGen.Generate("resilience:handler:throw:event"),
            AggregateId = registryId,
            Payload = new RegistrationActivatedEventSchema(registryId)
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(envelope));

        Assert.Equal(1, store.ReleaseCount);
    }

    [Fact]
    public async Task HandleAsync_AfterFailureAndRelease_CanRetrySuccessfully()
    {
        var registryId = IdGen.Generate("resilience:handler:retry");
        var eventId = IdGen.Generate("resilience:handler:retry:event");
        var store = new InMemoryIdempotencyStore();
        var envelope = new StubEnvelope
        {
            EventId = eventId,
            AggregateId = registryId,
            Payload = new RegistrationActivatedEventSchema(registryId)
        };

        var failDispatcher = new RecordingDispatcher(throwOn: new InvalidOperationException("transient"));
        var failHandler = BuildHandler(failDispatcher, store);
        await Assert.ThrowsAsync<InvalidOperationException>(() => failHandler.HandleAsync(envelope));

        var successDispatcher = new RecordingDispatcher();
        var retryHandler = BuildHandler(successDispatcher, store);
        await retryHandler.HandleAsync(envelope);

        Assert.Equal(2, successDispatcher.Dispatches.Count);
    }

    [Fact]
    public async Task HandleAsync_WithNonActivatedPayload_IsNoOp()
    {
        var dispatcher = new RecordingDispatcher();
        var store = new InMemoryIdempotencyStore();
        var handler = BuildHandler(dispatcher, store);

        var envelope = new StubEnvelope
        {
            EventId = IdGen.Generate("resilience:handler:wrong-type"),
            Payload = new RegistrationVerifiedEventSchema(Guid.NewGuid())
        };

        await handler.HandleAsync(envelope);

        Assert.Empty(dispatcher.Dispatches);
        Assert.Equal(0, store.ClaimAttempts);
    }

    [Fact]
    public async Task HandleAsync_ParticipantAndSubjectIds_AreDeterministic()
    {
        var registryId = IdGen.Generate("resilience:determinism:registry");
        var store1 = new InMemoryIdempotencyStore();
        var store2 = new InMemoryIdempotencyStore();
        var dispatcher1 = new RecordingDispatcher();
        var dispatcher2 = new RecordingDispatcher();

        var envelope1 = new StubEnvelope
        {
            EventId = IdGen.Generate("resilience:determinism:event"),
            AggregateId = registryId,
            Payload = new RegistrationActivatedEventSchema(registryId)
        };
        var envelope2 = new StubEnvelope
        {
            EventId = IdGen.Generate("resilience:determinism:event2"),
            AggregateId = registryId,
            Payload = new RegistrationActivatedEventSchema(registryId)
        };

        await BuildHandler(dispatcher1, store1).HandleAsync(envelope1);
        await BuildHandler(dispatcher2, store2).HandleAsync(envelope2);

        var p1 = (RegisterParticipantCommand)dispatcher1.Dispatches[0].Command;
        var p2 = (RegisterParticipantCommand)dispatcher2.Dispatches[0].Command;
        Assert.Equal(p1.ParticipantId, p2.ParticipantId);
    }
}
