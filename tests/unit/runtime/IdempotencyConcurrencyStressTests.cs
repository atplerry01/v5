using System.Collections.Concurrent;
using Whycespace.Runtime.Middleware.PostPolicy;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R1 Batch 5 — concurrency stress scaffold for the post-policy idempotency
/// middleware. Exercises the write-once-on-claim invariant under parallel
/// contention with a process-local <see cref="ConcurrentDictionary"/>-backed
/// <see cref="IIdempotencyStore"/>: 100 concurrent dispatches of the same
/// command must resolve to exactly one successful claim + 99 duplicate
/// rejections.
///
/// The test doubles as a contract pin for the R-IDEM-EVIDENCE-01 guarantees:
///   * <see cref="CommandContext.IdempotencyKey"/> stamped on every invocation.
///   * <see cref="CommandContext.IdempotencyOutcome"/> is <see cref="IdempotencyOutcome.Miss"/>
///     on the winning path and <see cref="IdempotencyOutcome.Hit"/> on duplicates.
///   * Duplicate result carries <see cref="CommandResult.IsDuplicate"/> = true
///     and <see cref="RuntimeFailureCategory.ConcurrencyConflict"/>.
///
/// NOT covered here (by design): distributed lease semantics, store crash
/// recovery, out-of-order retry — those land with R2.
/// </summary>
public sealed class IdempotencyConcurrencyStressTests
{
    private sealed record TestCommand(Guid Id);

    // Process-local idempotency store — atomic claim via ConcurrentDictionary.TryAdd.
    // Mirrors the semantic contract of PostgresIdempotencyStoreAdapter without the
    // network round-trip so we can measure contention at process speed.
    private sealed class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private readonly ConcurrentDictionary<string, byte> _claims = new();

        public Task<bool> TryClaimAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_claims.TryAdd(key, 0));

        public Task ReleaseAsync(string key, CancellationToken cancellationToken = default)
        {
            _claims.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        [Obsolete]
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_claims.ContainsKey(key));

        [Obsolete]
        public Task MarkAsync(string key, CancellationToken cancellationToken = default)
        {
            _claims.TryAdd(key, 0);
            return Task.CompletedTask;
        }
    }

    private static CommandContext NewContext(Guid commandId) => new()
    {
        CorrelationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        CausationId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        CommandId = commandId,
        TenantId = "stress-tenant",
        ActorId = "stress-actor",
        AggregateId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
        PolicyId = "default",
        Classification = "operational",
        Context = "sandbox",
        Domain = "stress"
    };

    [Fact]
    public async Task TryClaim_Under_100_Way_Contention_Resolves_To_Exactly_One_Winner()
    {
        // Shared idempotency key — all 100 tasks claim the SAME key so contention is maximal.
        var sharedCommandId = Guid.Parse("00000000-0000-0000-0000-00000000AAAA");
        var command = new TestCommand(sharedCommandId);

        var store = new InMemoryIdempotencyStore();
        var middleware = new IdempotencyMiddleware(store);

        const int concurrency = 100;
        var innerInvocations = 0;

        // Inner `next` increments a shared counter — in a correct implementation
        // exactly one invocation reaches this callback; all others short-circuit
        // at the claim gate with IsDuplicate=true.
        Func<CancellationToken, Task<CommandResult>> next = _ =>
        {
            Interlocked.Increment(ref innerInvocations);
            return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
        };

        var contexts = new CommandContext[concurrency];
        for (int i = 0; i < concurrency; i++)
            contexts[i] = NewContext(sharedCommandId);

        // Gate all tasks behind a barrier so they race at the claim call.
        using var startBarrier = new Barrier(concurrency);

        var tasks = new Task<CommandResult>[concurrency];
        for (int i = 0; i < concurrency; i++)
        {
            var ctx = contexts[i];
            tasks[i] = Task.Run(async () =>
            {
                startBarrier.SignalAndWait();
                return await middleware.ExecuteAsync(ctx, command, next, CancellationToken.None);
            });
        }

        var results = await Task.WhenAll(tasks);

        // Exactly one task must have reached `next`; all others must have
        // short-circuited on the duplicate path.
        Assert.Equal(1, innerInvocations);

        var winners = results.Where(r => r.IsSuccess).ToList();
        var duplicates = results.Where(r => r.IsDuplicate).ToList();

        Assert.Single(winners);
        Assert.Equal(concurrency - 1, duplicates.Count);

        // Every duplicate must carry the canonical shape per R-IDEM-EVIDENCE-01.
        foreach (var dup in duplicates)
        {
            Assert.True(dup.IsDuplicate);
            Assert.Equal(RuntimeFailureCategory.ConcurrencyConflict, dup.FailureCategory);
            Assert.False(string.IsNullOrEmpty(dup.IdempotencyKey));
        }

        // Every context must have the key stamped exactly once (write-once invariant).
        foreach (var ctx in contexts)
        {
            Assert.False(string.IsNullOrEmpty(ctx.IdempotencyKey));
            Assert.NotNull(ctx.IdempotencyOutcome);
        }

        // Exactly one context should have Miss; the rest should have Hit.
        var missCount = contexts.Count(c => c.IdempotencyOutcome == IdempotencyOutcome.Miss);
        var hitCount = contexts.Count(c => c.IdempotencyOutcome == IdempotencyOutcome.Hit);
        Assert.Equal(1, missCount);
        Assert.Equal(concurrency - 1, hitCount);
    }

    [Fact]
    public async Task Distinct_CommandIds_All_Claim_Independently_Under_Parallel_Load()
    {
        // Each task has a DISTINCT command id, so every claim should succeed.
        // This is the counter-test to the shared-key stress above — it proves
        // the middleware does not serialize unrelated commands.
        var store = new InMemoryIdempotencyStore();
        var middleware = new IdempotencyMiddleware(store);

        const int concurrency = 100;
        var innerInvocations = 0;

        Func<CancellationToken, Task<CommandResult>> next = _ =>
        {
            Interlocked.Increment(ref innerInvocations);
            return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
        };

        var tasks = Enumerable.Range(0, concurrency).Select(i =>
        {
            var commandId = Guid.NewGuid();
            var ctx = NewContext(commandId);
            var command = new TestCommand(commandId);
            return middleware.ExecuteAsync(ctx, command, next, CancellationToken.None);
        }).ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(concurrency, innerInvocations);
        Assert.All(results, r => Assert.True(r.IsSuccess));
        Assert.All(results, r => Assert.False(r.IsDuplicate));
    }
}
