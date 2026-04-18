using NSubstitute;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Phase8;

/// <summary>
/// Phase 8 B7 — end-to-end validation of the B5 sanction and system-lock
/// expiry schedulers. Exercises
/// <see cref="SanctionExpirySchedulerHandler"/> and
/// <see cref="SystemLockExpirySchedulerHandler"/> against an
/// <see cref="InMemoryExpirableSanctionQuery"/> /
/// <see cref="InMemoryExpirableLockQuery"/> double so the candidate-
/// selection / dispatch / idempotent-re-entry flow can be observed
/// without a real Postgres projection.
/// </summary>
public sealed class ExpirySchedulerTests
{
    // ── SANCTION ─────────────────────────────────────────────────

    [Fact]
    public async Task SanctionScheduler_ExpiredCandidate_DispatchesExpireSanctionCommand()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionExpirySchedulerHandler(dispatcher, idempotency);

        var sanctionId = Guid.Parse("11111111-aaaa-aaaa-aaaa-000000000001");
        var expiresAt = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);
        var candidate = new ExpirableSanctionCandidate(sanctionId, expiresAt);

        await handler.HandleAsync(candidate);

        await dispatcher.Received(1).DispatchSystemAsync(
            Arg.Is<ExpireSanctionCommand>(c =>
                c.SanctionId == sanctionId
                && c.ExpiredAt == expiresAt),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "enforcement"
                && r.Domain == "sanction"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SanctionScheduler_SameCandidateTwice_DispatchesOnlyOnce()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionExpirySchedulerHandler(dispatcher, idempotency);

        var sanctionId = Guid.Parse("11111111-aaaa-aaaa-aaaa-000000000002");
        var expiresAt = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);
        var candidate = new ExpirableSanctionCandidate(sanctionId, expiresAt);

        // Simulates two overlapping scheduler ticks (or a scan/restart/
        // re-scan sequence before the projection has caught up).
        await handler.HandleAsync(candidate);
        await handler.HandleAsync(candidate);

        await dispatcher.Received(1).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SanctionScheduler_DispatcherThrows_ReleasesClaimForRetry()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        var calls = 0;
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls++;
                return calls == 1
                    ? Task.FromException<CommandResult>(new InvalidOperationException("tx blip"))
                    : Task.FromResult(CommandResult.Success(Array.Empty<object>()));
            });

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionExpirySchedulerHandler(dispatcher, idempotency);

        var candidate = new ExpirableSanctionCandidate(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(candidate));
        await handler.HandleAsync(candidate);

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task SanctionQuery_OnlyReturnsCandidatesExpiredAtOrBeforeNow()
    {
        var query = new InMemoryExpirableSanctionQuery();
        var now = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        var expiredId = Guid.NewGuid();
        var futureId = Guid.NewGuid();
        query.Add(expiredId, now.AddMinutes(-1));
        query.Add(futureId, now.AddMinutes(+1));

        var result = await query.QueryExpirableAsync(now, batchSize: 100);

        Assert.Single(result);
        Assert.Equal(expiredId, result[0].SanctionId);
    }

    [Fact]
    public async Task SanctionQuery_BatchSizeBoundsResult_OrderedByExpiresAtAsc()
    {
        var query = new InMemoryExpirableSanctionQuery();
        var now = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        var oldest = Guid.NewGuid();
        var middle = Guid.NewGuid();
        var newest = Guid.NewGuid();
        query.Add(middle, now.AddMinutes(-5));
        query.Add(oldest, now.AddMinutes(-10));
        query.Add(newest, now.AddMinutes(-1));

        var result = await query.QueryExpirableAsync(now, batchSize: 2);

        Assert.Equal(2, result.Count);
        Assert.Equal(oldest, result[0].SanctionId);
        Assert.Equal(middle, result[1].SanctionId);
    }

    [Fact]
    public async Task SanctionScheduler_ClockAdvance_MovesCandidateFromFutureToExpired()
    {
        var query = new InMemoryExpirableSanctionQuery();
        var clock = new AdvanceableClock();

        var sanctionId = Guid.NewGuid();
        var expiresAt = clock.UtcNow.AddHours(1);
        query.Add(sanctionId, expiresAt);

        var beforeAdvance = await query.QueryExpirableAsync(clock.UtcNow, batchSize: 100);
        Assert.Empty(beforeAdvance);

        clock.Advance(TimeSpan.FromHours(2));

        var afterAdvance = await query.QueryExpirableAsync(clock.UtcNow, batchSize: 100);
        Assert.Single(afterAdvance);
        Assert.Equal(sanctionId, afterAdvance[0].SanctionId);
    }

    // ── SYSTEM LOCK ──────────────────────────────────────────────

    [Fact]
    public async Task LockScheduler_ExpiredCandidate_DispatchesExpireSystemLockCommand()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SystemLockExpirySchedulerHandler(dispatcher, idempotency);

        var lockId = Guid.Parse("22222222-bbbb-bbbb-bbbb-000000000001");
        var expiresAt = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);
        var candidate = new ExpirableLockCandidate(lockId, expiresAt);

        await handler.HandleAsync(candidate);

        await dispatcher.Received(1).DispatchSystemAsync(
            Arg.Is<ExpireSystemLockCommand>(c =>
                c.LockId == lockId
                && c.ExpiredAt == expiresAt),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "enforcement"
                && r.Domain == "lock"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LockQuery_SuspendedSimulation_RemovedCandidate_ReappearsAfterResume()
    {
        // The production query filters status='Locked' — Suspended rows
        // are naturally excluded. In the in-memory double we simulate
        // that by removing the candidate on suspend and re-adding on
        // resume. This test encodes the expected scheduler behaviour
        // across a suspend/resume cycle.
        var query = new InMemoryExpirableLockQuery();
        var now = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        var lockId = Guid.NewGuid();
        var expiresAt = now.AddMinutes(-1);
        query.Add(lockId, expiresAt);

        var beforeSuspend = await query.QueryExpirableAsync(now, 100);
        Assert.Single(beforeSuspend);

        // Simulate Suspend — projection flips status away from Locked,
        // scheduler no longer sees the row.
        query.Remove(lockId);

        var whileSuspended = await query.QueryExpirableAsync(now, 100);
        Assert.Empty(whileSuspended);

        // Simulate Resume — projection restores status='Locked', same
        // ExpiresAt preserved from original lock.
        query.Add(lockId, expiresAt);

        var afterResume = await query.QueryExpirableAsync(now, 100);
        Assert.Single(afterResume);
        Assert.Equal(lockId, afterResume[0].LockId);
    }

    [Fact]
    public async Task LockScheduler_SameCandidateTwice_DispatchesOnlyOnce()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SystemLockExpirySchedulerHandler(dispatcher, idempotency);

        var candidate = new ExpirableLockCandidate(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

        await handler.HandleAsync(candidate);
        await handler.HandleAsync(candidate);

        await dispatcher.Received(1).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
    }
}
