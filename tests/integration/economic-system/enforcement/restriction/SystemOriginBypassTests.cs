using Whycespace.Domain.EconomicSystem.Transaction.Wallet;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T2E.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Tests.Integration.EconomicSystem.Enforcement.Restriction;

/// <summary>
/// Phase 2.5 — system-origin bypass unit-integration. User-originated
/// commands on a restricted subject must be hard-rejected (Phase 2
/// contract preserved). System-origin commands on the same subject
/// must execute successfully so workflow compensation / settlement
/// completion / recovery can converge.
/// </summary>
public sealed class SystemOriginBypassTests
{
    private sealed class StubEngineContext : IEngineContext
    {
        private readonly List<object> _emitted = new();
        private readonly WalletAggregate? _preloaded;

        public StubEngineContext(
            object command,
            Guid aggregateId,
            string? enforcementConstraint,
            bool isSystem,
            WalletAggregate? preloaded = null)
        {
            Command = command;
            AggregateId = aggregateId;
            EnforcementConstraint = enforcementConstraint;
            IsSystem = isSystem;
            _preloaded = preloaded;
        }

        public object Command { get; }
        public Guid AggregateId { get; }
        public string? EnforcementConstraint { get; }
        public bool IsSystem { get; }
        public IReadOnlyList<object> EmittedEvents => _emitted;
        public Task<object> LoadAggregateAsync(Type aggregateType) =>
            _preloaded is null
                ? throw new InvalidOperationException("No preloaded aggregate.")
                : Task.FromResult<object>(_preloaded);
        public void EmitEvents(IReadOnlyList<object> events) => _emitted.AddRange(events);
    }

    private static WalletAggregate ActiveWallet(Guid walletId, Guid ownerId, Guid accountId, Timestamp createdAt)
    {
        var aggregate = (WalletAggregate)Activator.CreateInstance(typeof(WalletAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(new object[]
        {
            new WalletCreatedEvent(new WalletId(walletId), ownerId, accountId, createdAt)
        });
        return aggregate;
    }

    [Fact]
    public async Task Restricted_UserCommand_HardRejected()
    {
        // Phase 2 invariant preserved: user-originated command on a
        // restricted subject throws SubjectRestrictedException and emits
        // no events.
        var walletId = Guid.NewGuid();
        var ownerId  = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var createdAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
        var wallet = ActiveWallet(walletId, ownerId, accountId, createdAt);

        var cmd = new RequestWalletTransactionCommand(
            walletId, Guid.NewGuid(), Guid.NewGuid(), 100m, "USD",
            createdAt.Value);

        var ctx = new StubEngineContext(
            cmd, walletId,
            enforcementConstraint: "Restricted:Capital",
            isSystem: false,
            preloaded: wallet);

        var handler = new RequestWalletTransactionHandler();

        var ex = await Assert.ThrowsAsync<SubjectRestrictedException>(() => handler.ExecuteAsync(ctx));
        Assert.Equal("Capital", ex.Scope);
        Assert.Empty(ctx.EmittedEvents);
    }

    [Fact]
    public async Task Restricted_SystemCommand_Executes()
    {
        // Phase 2.5 — system-origin bypass. Identical subject state as
        // the user-command test above, but IsSystem=true. The handler
        // executes and the wallet emits TransactionRequestedEvent.
        var walletId = Guid.NewGuid();
        var ownerId  = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var createdAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
        var wallet = ActiveWallet(walletId, ownerId, accountId, createdAt);

        var cmd = new RequestWalletTransactionCommand(
            walletId, Guid.NewGuid(), Guid.NewGuid(), 100m, "USD",
            createdAt.Value);

        var ctx = new StubEngineContext(
            cmd, walletId,
            enforcementConstraint: "Restricted:Capital",
            isSystem: true,
            preloaded: wallet);

        var handler = new RequestWalletTransactionHandler();

        await handler.ExecuteAsync(ctx);

        Assert.NotEmpty(ctx.EmittedEvents);
        Assert.Contains(ctx.EmittedEvents, e => e is TransactionRequestedEvent);
    }

    [Fact]
    public async Task Unrestricted_SystemCommand_Executes()
    {
        // Sanity: IsSystem=true with no constraint still executes
        // normally. The bypass does not depend on the constraint state.
        var walletId = Guid.NewGuid();
        var ownerId  = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var createdAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
        var wallet = ActiveWallet(walletId, ownerId, accountId, createdAt);

        var cmd = new RequestWalletTransactionCommand(
            walletId, Guid.NewGuid(), Guid.NewGuid(), 100m, "USD",
            createdAt.Value);

        var ctx = new StubEngineContext(
            cmd, walletId,
            enforcementConstraint: null,
            isSystem: true,
            preloaded: wallet);

        var handler = new RequestWalletTransactionHandler();

        await handler.ExecuteAsync(ctx);

        Assert.NotEmpty(ctx.EmittedEvents);
    }

    [Fact]
    public async Task Unrestricted_UserCommand_Executes()
    {
        // Regression check: Phase 2.5 must not block previously-allowed
        // user commands on non-restricted subjects.
        var walletId = Guid.NewGuid();
        var ownerId  = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var createdAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
        var wallet = ActiveWallet(walletId, ownerId, accountId, createdAt);

        var cmd = new RequestWalletTransactionCommand(
            walletId, Guid.NewGuid(), Guid.NewGuid(), 100m, "USD",
            createdAt.Value);

        var ctx = new StubEngineContext(
            cmd, walletId,
            enforcementConstraint: null,
            isSystem: false,
            preloaded: wallet);

        var handler = new RequestWalletTransactionHandler();

        await handler.ExecuteAsync(ctx);

        Assert.NotEmpty(ctx.EmittedEvents);
    }
}
