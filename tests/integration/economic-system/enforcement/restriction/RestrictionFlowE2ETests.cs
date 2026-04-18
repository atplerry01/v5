using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T2E.Economic.Enforcement.Restriction;
using Whycespace.Engines.T2E.Economic.Transaction.Wallet;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.Middleware.Execution;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.EconomicSystem.Enforcement.Restriction;

/// <summary>
/// Phase 2 — T2.5 in-process enforcement flow. Wires ApplyRestrictionHandler
/// + ExecutionGuardMiddleware + RequestWalletTransactionHandler against the
/// real <see cref="InMemoryEnforcementDecisionCache"/> and proves the
/// Phase-2 locked contract:
///
///   1. A restriction is applied against a subject.
///   2. The very next <c>RequestWalletTransactionCommand</c> for that
///      subject hits the middleware, sees the restriction in-cache, and
///      is hard-rejected before any handler executes.
///   3. No events are emitted (no ledger effect, no workflow progression).
///   4. When the restriction is removed, the same command succeeds
///      through the handler (fresh wallet aggregate path, no restriction
///      stamped).
///
/// Uses in-process components only — no HTTP, Postgres, or Kafka — so the
/// test runs in the Unit project alongside the Phase-1 invariant suites.
/// </summary>
public sealed class RestrictionFlowE2ETests
{
    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);
    }

    private static CommandContext BuildContext(Guid subjectId, Guid aggregateId, string policyId) => new()
    {
        CorrelationId = Guid.NewGuid(),
        CausationId   = Guid.NewGuid(),
        CommandId     = Guid.NewGuid(),
        TenantId      = "tenant",
        ActorId       = "actor",
        AggregateId   = aggregateId,
        PolicyId      = policyId,
        Classification = "economic",
        Context        = "transaction",
        Domain         = "wallet",
        PolicyDecisionAllowed = true,
        PolicyDecisionHash    = "hash",
        IdentityId            = subjectId.ToString(),
    };

    private sealed class StubEngineContext : IEngineContext
    {
        private readonly List<object> _emitted = new();
        public StubEngineContext(
            object command,
            Guid aggregateId,
            string? enforcementConstraint,
            object? preloaded = null,
            bool isSystem = false)
        {
            Command = command;
            AggregateId = aggregateId;
            EnforcementConstraint = enforcementConstraint;
            IsSystem = isSystem;
            _preloaded = preloaded;
        }
        private readonly object? _preloaded;
        public object Command { get; }
        public Guid AggregateId { get; }
        public string? EnforcementConstraint { get; }
        public bool IsSystem { get; }
        public IReadOnlyList<object> EmittedEvents => _emitted;
        public Task<object> LoadAggregateAsync(Type aggregateType) =>
            _preloaded is null
                ? throw new InvalidOperationException("No preloaded aggregate.")
                : Task.FromResult(_preloaded);
        public void EmitEvents(IReadOnlyList<object> events) => _emitted.AddRange(events);
    }

    [Fact]
    public async Task ApplyRestriction_PopulatesCache_NextCommandHardRejected()
    {
        var clock = new FixedClock();
        var cache = new InMemoryEnforcementDecisionCache(clock);

        var subjectId = Guid.NewGuid();
        var restrictionId = Guid.NewGuid();

        // 1. ApplyRestrictionHandler populates the cache via its injected dep.
        //    Event store is empty — this is a fresh issuance, so the handler's
        //    prior-event idempotency check (DOM-LIFECYCLE-INIT-IDEMPOTENT-01.b)
        //    short-circuits to "proceed" on the first invocation.
        var applyHandler = new ApplyRestrictionHandler(new InMemoryEventStore(), cache);
        var applyCmd = new ApplyRestrictionCommand(
            restrictionId, subjectId, "Capital", "fraud", clock.UtcNow);
        var applyCtx = new StubEngineContext(applyCmd, restrictionId, enforcementConstraint: null);
        await applyHandler.ExecuteAsync(applyCtx);

        Assert.NotEmpty(applyCtx.EmittedEvents); // RestrictionAppliedEvent emitted
        var cached = cache.TryGetRestriction(subjectId);
        Assert.NotNull(cached);
        Assert.True(cached!.IsRestricted);
        Assert.Equal("Capital", cached.Scope);

        // 2. The next command for the same subject reaches the middleware.
        var middleware = new ExecutionGuardMiddleware(
            violationStateQuery: null,
            escalationStateQuery: null,
            restrictionStateQuery: null,
            lockStateQuery: null,
            decisionCache: cache,
            logger: null);

        var walletId = Guid.NewGuid();
        var walletCmd = new RequestWalletTransactionCommand(
            walletId, Guid.NewGuid(), Guid.NewGuid(), 100m, "USD", clock.UtcNow);
        var walletCtx = BuildContext(subjectId, walletId,
            "economic.transaction.wallet.request_transaction");

        bool handlerReached = false;
        var result = await middleware.ExecuteAsync(
            walletCtx, walletCmd,
            _ => { handlerReached = true; return Task.FromResult(CommandResult.Success(Array.Empty<object>())); });

        // 3. Hard-rejected. Handler NEVER reached. No events emitted.
        Assert.False(result.IsSuccess);
        Assert.False(handlerReached, "restriction must reject before the wallet handler runs.");
        Assert.Contains("restricted", result.Error!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Restricted:Capital", walletCtx.EnforcementConstraint);
    }

    [Fact]
    public async Task RemoveRestriction_ClearsCache_NextCommandProceeds()
    {
        var clock = new FixedClock();
        var cache = new InMemoryEnforcementDecisionCache(clock);

        var subjectId = Guid.NewGuid();
        var restrictionId = Guid.NewGuid();

        // Apply, then Remove — both handlers update the cache deterministically.
        var applyHandler = new ApplyRestrictionHandler(new InMemoryEventStore(), cache);
        var applyCmd = new ApplyRestrictionCommand(
            restrictionId, subjectId, "Capital", "fraud", clock.UtcNow);
        var applyCtx = new StubEngineContext(applyCmd, restrictionId, enforcementConstraint: null);
        await applyHandler.ExecuteAsync(applyCtx);

        // Rehydrate the aggregate so the RemoveRestrictionHandler can load it.
        var rehydrated = (RestrictionAggregate)Activator.CreateInstance(
            typeof(RestrictionAggregate), nonPublic: true)!;
        rehydrated.LoadFromHistory(applyCtx.EmittedEvents);

        var removeHandler = new RemoveRestrictionHandler(cache);
        var removeCmd = new RemoveRestrictionCommand(
            restrictionId, "cleared", clock.UtcNow.AddHours(1));
        var removeCtx = new StubEngineContext(removeCmd, restrictionId,
            enforcementConstraint: null,
            preloaded: rehydrated);
        await removeHandler.ExecuteAsync(removeCtx);

        Assert.Null(cache.TryGetRestriction(subjectId));

        // Next command should flow through the middleware.
        var middleware = new ExecutionGuardMiddleware(
            violationStateQuery: null,
            escalationStateQuery: null,
            restrictionStateQuery: null,
            lockStateQuery: null,
            decisionCache: cache,
            logger: null);

        var walletId = Guid.NewGuid();
        var walletCmd = new RequestWalletTransactionCommand(
            walletId, Guid.NewGuid(), Guid.NewGuid(), 100m, "USD", clock.UtcNow);
        var walletCtx = BuildContext(subjectId, walletId,
            "economic.transaction.wallet.request_transaction");

        bool handlerReached = false;
        var result = await middleware.ExecuteAsync(
            walletCtx, walletCmd,
            _ => { handlerReached = true; return Task.FromResult(CommandResult.Success(Array.Empty<object>())); });

        Assert.True(result.IsSuccess);
        Assert.True(handlerReached);
        Assert.Null(walletCtx.EnforcementConstraint);
    }

    [Fact]
    public async Task DefenseInDepth_HandlerThrowsSubjectRestricted_WhenMiddlewareBypassed()
    {
        // If a caller skips the middleware but still sets EnforcementConstraint
        // on the engine context (e.g. workflow recovery code), the handler
        // itself enforces via EnforcementGuard. Exercised via the wallet
        // RequestTransaction handler since it is the primary money-movement
        // surface on Phase 1.
        var walletId = Guid.NewGuid();
        var cmd = new RequestWalletTransactionCommand(
            walletId, Guid.NewGuid(), Guid.NewGuid(), 100m, "USD",
            new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

        var ctx = new StubEngineContext(
            command: cmd,
            aggregateId: walletId,
            enforcementConstraint: "Restricted:Capital");

        var handler = new RequestWalletTransactionHandler();

        var ex = await Assert.ThrowsAsync<SubjectRestrictedException>(
            () => handler.ExecuteAsync(ctx));
        Assert.Empty(ctx.EmittedEvents);
        Assert.Equal("Capital", ex.Scope);
    }
}
