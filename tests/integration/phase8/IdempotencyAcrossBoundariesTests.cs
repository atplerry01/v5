using NSubstitute;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Phase8;

/// <summary>
/// Phase 8 B7 — cross-boundary idempotency validation. Asserts that the
/// deterministic-key contracts across B3 (reactor envelope claim), B4
/// (reactor envelope claim), and B5 (scheduler per-candidate claim)
/// produce distinct, collision-free key namespaces while each layer
/// independently blocks duplicate dispatches.
///
/// <para>
/// These tests complement the per-batch reactor / scheduler test files
/// by exercising CROSS-layer invariants — e.g. "a reactor claim for
/// sanction activation must not accidentally block a scheduler claim
/// for the same sanction's expiry", which is only visible when both
/// layers share an <see cref="IIdempotencyStore"/>.
/// </para>
/// </summary>
public sealed class IdempotencyAcrossBoundariesTests
{
    [Fact]
    public async Task SharedIdempotencyStore_SanctionActivationReactor_And_SanctionExpiryScheduler_DoNotCollide()
    {
        // One shared store — mirrors the production composition where
        // both the activation reactor and the expiry scheduler resolve
        // the same IIdempotencyStore singleton.
        var idempotency = new InMemoryIdempotencyStore();

        var reactorDispatcher = Substitute.For<ISystemIntentDispatcher>();
        reactorDispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));
        var reactor = new SanctionActivationEnforcementHandler(reactorDispatcher, idempotency);

        var schedulerDispatcher = Substitute.For<ISystemIntentDispatcher>();
        schedulerDispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));
        var scheduler = new SanctionExpirySchedulerHandler(schedulerDispatcher, idempotency);

        var sanctionId = Guid.Parse("77777777-0000-0000-0000-000000000001");
        var subjectId = Guid.NewGuid();
        var enforcementId = Guid.NewGuid();
        var activatedAt = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);
        var expiresAt = activatedAt.AddDays(1);

        // Reactor dispatches for the sanction's activation event.
        var activatedEnvelope = new RawTestEnvelope
        {
            EventId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            AggregateId = sanctionId,
            Payload = new SanctionActivatedEventSchema(sanctionId, subjectId, activatedAt)
            {
                Enforcement = new SanctionEnforcementRefDto("Restriction", enforcementId)
            }
        };
        await reactor.HandleAsync(activatedEnvelope);

        // Scheduler dispatches later for the SAME sanction's expiry.
        await scheduler.HandleAsync(new ExpirableSanctionCandidate(sanctionId, expiresAt));

        // Both dispatches succeed — the reactor key ("sanction-activation-
        // enforcement:{EventId}") and the scheduler key ("sanction-expiry:
        // {SanctionId:N}:{UtcTicks}") live in disjoint namespaces.
        await reactorDispatcher.Received(1).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
        await schedulerDispatcher.Received(1).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SchedulerKey_IsDeterministic_PerAggregateAndExpiresAt()
    {
        // Two scheduler handlers with two separate idempotency stores
        // (simulating a restart where the in-memory claim is lost) both
        // see the SAME candidate. Because the scheduler key is a pure
        // function of (SanctionId, ExpiresAt.UtcTicks), the second
        // handler also claims successfully and dispatches — which is
        // the correct behaviour across a full claim-store wipe. The
        // aggregate's Status != Active guard is the last-line-of-
        // defence in that (rare) scenario; tested at the aggregate
        // unit-test layer.
        //
        // What THIS test pins is the KEY DETERMINISM property: the same
        // (SanctionId, ExpiresAt) pair always produces the same string
        // key, so two handlers sharing ONE store collapse to one
        // dispatch.
        var sharedStore = new InMemoryIdempotencyStore();

        var dispatcherA = Substitute.For<ISystemIntentDispatcher>();
        dispatcherA.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));
        var dispatcherB = Substitute.For<ISystemIntentDispatcher>();
        dispatcherB.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var handlerA = new SanctionExpirySchedulerHandler(dispatcherA, sharedStore);
        var handlerB = new SanctionExpirySchedulerHandler(dispatcherB, sharedStore);

        var candidate = new ExpirableSanctionCandidate(
            SanctionId: Guid.Parse("88888888-0000-0000-0000-000000000001"),
            ExpiresAt: new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

        await handlerA.HandleAsync(candidate);
        await handlerB.HandleAsync(candidate);

        // Only ONE handler dispatched — the deterministic key caused
        // B's TryClaim to return false.
        var aCount = dispatcherA.ReceivedCalls()
            .Count(c => c.GetMethodInfo().Name == nameof(ISystemIntentDispatcher.DispatchSystemAsync));
        var bCount = dispatcherB.ReceivedCalls()
            .Count(c => c.GetMethodInfo().Name == nameof(ISystemIntentDispatcher.DispatchSystemAsync));
        Assert.Equal(1, aCount + bCount);
    }

    [Fact]
    public async Task SanctionExpiryKey_DiffersFromLockExpiryKey_EvenForIdenticalGuidAndExpiresAt()
    {
        // Prefix discipline — the sanction scheduler uses
        // "sanction-expiry:" and the lock scheduler uses
        // "system-lock-expiry:" so a Guid shared by a sanction id and
        // a lock id CANNOT accidentally collide in the idempotency
        // store. Both dispatches must succeed.
        var sharedStore = new InMemoryIdempotencyStore();

        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var sanctionHandler = new SanctionExpirySchedulerHandler(dispatcher, sharedStore);
        var lockHandler = new SystemLockExpirySchedulerHandler(dispatcher, sharedStore);

        var sharedGuid = Guid.Parse("99999999-0000-0000-0000-000000000001");
        var expiresAt = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        await sanctionHandler.HandleAsync(new ExpirableSanctionCandidate(sharedGuid, expiresAt));
        await lockHandler.HandleAsync(new ExpirableLockCandidate(sharedGuid, expiresAt));

        await dispatcher.Received(2).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactorKey_DiffersAcrossSaga_EvenForIdenticalEventId()
    {
        // A single EventId shared across the sanction-activation topic
        // and the payout-failure topic must NOT accidentally claim each
        // other — reactor keys are prefixed so the claims are disjoint.
        var sharedStore = new InMemoryIdempotencyStore();

        var commandDispatcher = Substitute.For<ISystemIntentDispatcher>();
        commandDispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));
        var sanctionReactor = new SanctionActivationEnforcementHandler(commandDispatcher, sharedStore);

        var workflowDispatcher = Substitute.For<IWorkflowDispatcher>();
        workflowDispatcher.StartWorkflowAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DomainRoute>())
            .Returns(Task.FromResult(WorkflowResult.Success()));
        var payoutReactor = new PayoutFailureCompensationIntegrationHandler(
            workflowDispatcher, sharedStore);

        var sharedEventId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var sanctionEnvelope = new RawTestEnvelope
        {
            EventId = sharedEventId,
            AggregateId = Guid.NewGuid(),
            Payload = new SanctionActivatedEventSchema(
                AggregateId: Guid.NewGuid(),
                SubjectId: Guid.NewGuid(),
                ActivatedAt: DateTimeOffset.UnixEpoch)
            {
                Enforcement = new SanctionEnforcementRefDto("Restriction", Guid.NewGuid())
            }
        };

        var payoutEnvelope = new RawTestEnvelope
        {
            EventId = sharedEventId,
            AggregateId = Guid.NewGuid(),
            Payload = new PayoutFailedEventSchema(
                AggregateId: Guid.NewGuid(),
                DistributionId: Guid.NewGuid(),
                Reason: "x",
                FailedAt: DateTimeOffset.UnixEpoch)
        };

        await sanctionReactor.HandleAsync(sanctionEnvelope);
        await payoutReactor.HandleAsync(payoutEnvelope);

        await commandDispatcher.Received(1).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
        await workflowDispatcher.Received(1).StartWorkflowAsync(
            Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DomainRoute>());
    }
}
