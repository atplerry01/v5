using Microsoft.Extensions.Configuration;
using Whycespace.Runtime.Bootstrap;
using Whycespace.Runtime.Security;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Bootstrap;

/// <summary>
/// 2.8.25 — Platform bootstrap service unit tests.
///
/// Covers:
///  - blank email/type args → throws ArgumentException
///  - event store has events → dispatch skipped (idempotent)
///  - event store empty → dispatch called with deterministic RegistryId
///  - same config twice → same RegistryId (determinism)
///  - different email → different RegistryId
///  - dispatch failure → exception propagates
///  - dispatch throws → exception re-thrown
///  - StartAsync sets SystemIdentityScope before dispatch
/// </summary>
public sealed class PlatformBootstrapServiceTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly TestClock Clock = new();

    // ── Test doubles ──────────────────────────────────────────────────────────

    private sealed class StubEventStore : IEventStore
    {
        private readonly int _existingCount;

        public StubEventStore(int existingCount = 0) => _existingCount = existingCount;

        public Task<IReadOnlyList<object>> LoadEventsAsync(
            Guid aggregateId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<object>>(
                Enumerable.Range(0, _existingCount).Select(_ => (object)new object()).ToList());

        public Task AppendEventsAsync(
            Guid aggregateId,
            IReadOnlyList<IEventEnvelope> envelopes,
            int expectedVersion,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
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

    private sealed class FailingDispatcher : ISystemIntentDispatcher
    {
        public Task<CommandResult> DispatchAsync(object command, DomainRoute route, CancellationToken ct = default)
            => Task.FromResult(CommandResult.Success(Array.Empty<object>()));

        public Task<CommandResult> DispatchSystemAsync(object command, DomainRoute route, CancellationToken ct = default)
            => Task.FromResult(CommandResult.Failure("policy evaluation failed"));
    }

    private static PlatformBootstrapService Build(
        ISystemIntentDispatcher dispatcher,
        IEventStore eventStore,
        string email = "operator@platform.test",
        string type = "Operator")
        => new(dispatcher, IdGen, Clock, eventStore, email, type);

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_BlankEmail_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => Build(new RecordingDispatcher(), new StubEventStore(), email: ""));
        Assert.Contains("operatorEmail", ex.Message);
    }

    [Fact]
    public void Constructor_BlankRegistrationType_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => Build(new RecordingDispatcher(), new StubEventStore(), type: ""));
        Assert.Contains("registrationType", ex.Message);
    }

    [Fact]
    public async Task StartAsync_WhenEventsExist_SkipsDispatch()
    {
        var dispatcher = new RecordingDispatcher();
        var service = Build(dispatcher, new StubEventStore(existingCount: 3));

        await service.StartAsync(CancellationToken.None);

        Assert.Empty(dispatcher.Dispatches);
    }

    [Fact]
    public async Task StartAsync_WhenNoEvents_DispatchesCommand()
    {
        var dispatcher = new RecordingDispatcher();
        var service = Build(dispatcher, new StubEventStore(existingCount: 0));

        await service.StartAsync(CancellationToken.None);

        Assert.Single(dispatcher.Dispatches);
        var (cmd, route) = dispatcher.Dispatches[0];
        var initiate = Assert.IsType<InitiateRegistrationCommand>(cmd);
        Assert.Equal("operator@platform.test", initiate.Email);
        Assert.Equal("Operator", initiate.RegistrationType);
        Assert.Equal("trust", route.Classification);
        Assert.Equal("identity", route.Context);
        Assert.Equal("registry", route.Domain);
    }

    [Fact]
    public async Task StartAsync_DeterministicRegistryId_SameInputProducesSameId()
    {
        var dispatcher1 = new RecordingDispatcher();
        var dispatcher2 = new RecordingDispatcher();

        await Build(dispatcher1, new StubEventStore(), "alice@test.com", "Individual")
            .StartAsync(CancellationToken.None);
        await Build(dispatcher2, new StubEventStore(), "alice@test.com", "Individual")
            .StartAsync(CancellationToken.None);

        var id1 = ((InitiateRegistrationCommand)dispatcher1.Dispatches[0].Command).RegistryId;
        var id2 = ((InitiateRegistrationCommand)dispatcher2.Dispatches[0].Command).RegistryId;
        Assert.Equal(id1, id2);
    }

    [Fact]
    public async Task StartAsync_DifferentEmail_ProducesDifferentId()
    {
        var dispatcher1 = new RecordingDispatcher();
        var dispatcher2 = new RecordingDispatcher();

        await Build(dispatcher1, new StubEventStore(), "alice@test.com").StartAsync(CancellationToken.None);
        await Build(dispatcher2, new StubEventStore(), "bob@test.com").StartAsync(CancellationToken.None);

        var id1 = ((InitiateRegistrationCommand)dispatcher1.Dispatches[0].Command).RegistryId;
        var id2 = ((InitiateRegistrationCommand)dispatcher2.Dispatches[0].Command).RegistryId;
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task StartAsync_DispatchReturnsFailure_Throws()
    {
        var service = Build(new FailingDispatcher(), new StubEventStore());
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartAsync(CancellationToken.None));
        Assert.Contains("bootstrap failed", ex.Message);
    }

    [Fact]
    public async Task StartAsync_DispatchThrows_PropagatesException()
    {
        var dispatcher = new RecordingDispatcher(
            throwOn: new InvalidOperationException("kafka unavailable"));
        var service = Build(dispatcher, new StubEventStore());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_SetsSystemIdentityScope_BeforeDispatch()
    {
        SystemIdentityScope? capturedScope = null;
        var dispatcher = new CapturingDispatcher(scope => capturedScope = scope);
        var service = Build(dispatcher, new StubEventStore());

        await service.StartAsync(CancellationToken.None);

        Assert.NotNull(capturedScope);
        Assert.Equal("system/platform-bootstrap", capturedScope!.ActorId);
        Assert.Equal("system", capturedScope.TenantId);
    }

    private sealed class CapturingDispatcher : ISystemIntentDispatcher
    {
        private readonly Action<SystemIdentityScope?> _capture;
        public CapturingDispatcher(Action<SystemIdentityScope?> capture) => _capture = capture;

        public Task<CommandResult> DispatchAsync(object command, DomainRoute route, CancellationToken ct = default)
            => Task.FromResult(CommandResult.Success(Array.Empty<object>()));

        public Task<CommandResult> DispatchSystemAsync(object command, DomainRoute route, CancellationToken ct = default)
        {
            _capture(SystemIdentityScope.Current);
            return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
        }
    }
}
