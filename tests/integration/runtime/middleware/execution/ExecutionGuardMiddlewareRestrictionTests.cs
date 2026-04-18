using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.Middleware.Execution;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Integration.Runtime.Middleware.Execution;

/// <summary>
/// Phase 2 — T2.2 + T2.6 coverage: the execution-guard middleware must
/// hard-reject restricted subjects <em>before</em> dispatch and must
/// reflect cache updates within the same request window. The tests
/// exercise the full middleware decision path end-to-end against the
/// real in-memory cache + a no-op next-delegate spy.
/// </summary>
public sealed class ExecutionGuardMiddlewareRestrictionTests
{
    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);
    }

    private static CommandContext BuildContext(Guid subjectId) => new()
    {
        CorrelationId = Guid.NewGuid(),
        CausationId   = Guid.NewGuid(),
        CommandId     = Guid.NewGuid(),
        TenantId      = "tenant",
        ActorId       = "actor",
        AggregateId   = Guid.NewGuid(),
        PolicyId      = "economic.transaction.wallet.request_transaction",
        Classification = "economic",
        Context        = "transaction",
        Domain         = "wallet",
        PolicyDecisionAllowed = true,
        PolicyDecisionHash    = "deterministic-hash",
        IdentityId            = subjectId.ToString(),
    };

    private sealed record FakeCommand(Guid WalletId);

    [Fact]
    public async Task CachedRestriction_HardRejects_BeforeNextMiddleware()
    {
        // T2.6 — cache populated synchronously by ApplyRestrictionHandler
        // must block the very next command on the same subject, no
        // projection round-trip required.
        var clock = new FixedClock();
        var cache = new InMemoryEnforcementDecisionCache(clock);
        var middleware = new ExecutionGuardMiddleware(
            violationStateQuery: null,
            escalationStateQuery: null,
            restrictionStateQuery: null,
            lockStateQuery: null,
            decisionCache: cache,
            logger: null);

        var subjectId = Guid.NewGuid();
        cache.RecordRestriction(subjectId,
            new ActiveRestrictionState(IsRestricted: true, Scope: "Capital", Reason: "fraud"));

        var context = BuildContext(subjectId);
        var nextCalled = false;

        var result = await middleware.ExecuteAsync(
            context,
            new FakeCommand(Guid.NewGuid()),
            _ => { nextCalled = true; return Task.FromResult(CommandResult.Success(Array.Empty<object>())); });

        Assert.False(result.IsSuccess);
        Assert.False(nextCalled, "next() must not run when the subject is restricted.");
        Assert.Contains("restricted", result.Error!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Capital", result.Error!, StringComparison.Ordinal);

        // The stamped constraint matches the EnforcementGuard prefix so any
        // downstream bypass path that reaches a handler also hard-rejects.
        Assert.Equal("Restricted:Capital", context.EnforcementConstraint);
    }

    [Fact]
    public async Task NoRestriction_AllowsDispatch()
    {
        var cache = new InMemoryEnforcementDecisionCache(new FixedClock());
        var middleware = new ExecutionGuardMiddleware(
            violationStateQuery: null,
            escalationStateQuery: null,
            restrictionStateQuery: null,
            lockStateQuery: null,
            decisionCache: cache,
            logger: null);

        var context = BuildContext(Guid.NewGuid());
        var nextCalled = false;

        var result = await middleware.ExecuteAsync(
            context,
            new FakeCommand(Guid.NewGuid()),
            _ => { nextCalled = true; return Task.FromResult(CommandResult.Success(Array.Empty<object>())); });

        Assert.True(result.IsSuccess);
        Assert.True(nextCalled);
        Assert.Null(context.EnforcementConstraint);
    }

    [Fact]
    public async Task RestrictionThenClear_AllowsFollowUpCommand()
    {
        // ApplyRestrictionHandler records; RemoveRestrictionHandler clears.
        // The very next command after a clear must pass through.
        var cache = new InMemoryEnforcementDecisionCache(new FixedClock());
        var middleware = new ExecutionGuardMiddleware(
            violationStateQuery: null,
            escalationStateQuery: null,
            restrictionStateQuery: null,
            lockStateQuery: null,
            decisionCache: cache,
            logger: null);

        var subjectId = Guid.NewGuid();
        cache.RecordRestriction(subjectId,
            new ActiveRestrictionState(IsRestricted: true, Scope: "Capital", Reason: null));
        cache.ClearRestriction(subjectId);

        var context = BuildContext(subjectId);
        var result = await middleware.ExecuteAsync(
            context,
            new FakeCommand(Guid.NewGuid()),
            _ => Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CachedRestrictionNotRestricted_IsIgnored()
    {
        // Only IsRestricted=true entries hard-reject. A None-state entry
        // (IsRestricted=false) must allow the command through.
        var cache = new InMemoryEnforcementDecisionCache(new FixedClock());
        var middleware = new ExecutionGuardMiddleware(
            violationStateQuery: null,
            escalationStateQuery: null,
            restrictionStateQuery: null,
            lockStateQuery: null,
            decisionCache: cache,
            logger: null);

        var subjectId = Guid.NewGuid();
        cache.RecordRestriction(subjectId, ActiveRestrictionState.None);

        var context = BuildContext(subjectId);
        var result = await middleware.ExecuteAsync(
            context,
            new FakeCommand(Guid.NewGuid()),
            _ => Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        Assert.True(result.IsSuccess);
    }
}
