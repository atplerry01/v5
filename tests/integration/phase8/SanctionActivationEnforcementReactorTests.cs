using NSubstitute;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Phase8;

/// <summary>
/// Phase 8 B7 — end-to-end validation of the B4 sanction-activation
/// enforcement saga reactor. Exercises
/// <see cref="SanctionActivationEnforcementHandler"/> with a synthetic
/// <see cref="SanctionActivatedEventSchema"/> envelope and an
/// NSubstitute-mocked <see cref="ISystemIntentDispatcher"/> so the
/// handler's routing, idempotency, and failure-release behaviours are
/// observed in isolation — no Kafka, no real runtime pipeline needed
/// for this layer.
/// </summary>
public sealed class SanctionActivationEnforcementReactorTests
{
    private static RawTestEnvelope BuildEnvelope(
        Guid sanctionId,
        Guid subjectId,
        SanctionEnforcementRefDto? enforcement,
        DateTimeOffset activatedAt,
        Guid? eventId = null) =>
        new()
        {
            EventId = eventId ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            AggregateId = sanctionId,
            CorrelationId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            EventType = nameof(SanctionActivatedEventSchema),
            Payload = new SanctionActivatedEventSchema(sanctionId, subjectId, activatedAt)
            {
                Enforcement = enforcement
            }
        };

    [Fact]
    public async Task RestrictionKind_DispatchesApplyRestrictionCommand_WithEnforcementIdAsAggregate()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionActivationEnforcementHandler(dispatcher, idempotency);

        var sanctionId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
        var subjectId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
        var restrictionId = Guid.Parse("cccccccc-0000-0000-0000-000000000003");
        var activatedAt = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        var envelope = BuildEnvelope(
            sanctionId, subjectId,
            new SanctionEnforcementRefDto("Restriction", restrictionId),
            activatedAt);

        await handler.HandleAsync(envelope);

        await dispatcher.Received(1).DispatchSystemAsync(
            Arg.Is<ApplyRestrictionCommand>(c =>
                c.RestrictionId == restrictionId
                && c.SubjectId == subjectId
                && c.AppliedAt == activatedAt),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "enforcement"
                && r.Domain == "restriction"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LockKind_DispatchesLockSystemCommand_WithEnforcementIdAsAggregate()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionActivationEnforcementHandler(dispatcher, idempotency);

        var sanctionId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000011");
        var subjectId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000012");
        var lockId = Guid.Parse("dddddddd-0000-0000-0000-000000000013");
        var activatedAt = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        var envelope = BuildEnvelope(
            sanctionId, subjectId,
            new SanctionEnforcementRefDto("Lock", lockId),
            activatedAt);

        await handler.HandleAsync(envelope);

        await dispatcher.Received(1).DispatchSystemAsync(
            Arg.Is<LockSystemCommand>(c =>
                c.LockId == lockId
                && c.SubjectId == subjectId
                && c.LockedAt == activatedAt),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "enforcement"
                && r.Domain == "lock"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task V1Envelope_NullEnforcement_SkipsDispatchExplicitly()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionActivationEnforcementHandler(dispatcher, idempotency);

        var envelope = BuildEnvelope(
            sanctionId: Guid.NewGuid(),
            subjectId: Guid.NewGuid(),
            enforcement: null,
            activatedAt: DateTimeOffset.UnixEpoch);

        await handler.HandleAsync(envelope);

        await dispatcher.DidNotReceiveWithAnyArgs()
            .DispatchSystemAsync(default!, default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NonActivationPayload_IsObservationOnly_NoDispatchNoClaim()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionActivationEnforcementHandler(dispatcher, idempotency);

        // An arbitrary payload that is not SanctionActivatedEventSchema —
        // the reactor is strictly typed on the activation schema.
        var envelope = new RawTestEnvelope
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            Payload = new SanctionExpiredEventSchema(
                AggregateId: Guid.NewGuid(),
                SubjectId: Guid.NewGuid(),
                ExpiredAt: DateTimeOffset.UnixEpoch)
        };

        await handler.HandleAsync(envelope);

        await dispatcher.DidNotReceiveWithAnyArgs()
            .DispatchSystemAsync(default!, default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SameEventId_TwiceDelivered_DispatchesOnlyOnce()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionActivationEnforcementHandler(dispatcher, idempotency);

        var eventId = Guid.Parse("eeeeeeee-0000-0000-0000-000000000042");
        var envelope = BuildEnvelope(
            sanctionId: Guid.NewGuid(),
            subjectId: Guid.NewGuid(),
            enforcement: new SanctionEnforcementRefDto("Restriction", Guid.NewGuid()),
            activatedAt: DateTimeOffset.UnixEpoch,
            eventId: eventId);

        await handler.HandleAsync(envelope);
        await handler.HandleAsync(envelope);

        await dispatcher.Received(1).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatcherThrows_ReleasesClaim_NextAttemptDispatchesCleanly()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        var callCount = 0;
        dispatcher.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return callCount == 1
                    ? Task.FromException<CommandResult>(new InvalidOperationException("simulated dispatch failure"))
                    : Task.FromResult(CommandResult.Success(Array.Empty<object>()));
            });

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new SanctionActivationEnforcementHandler(dispatcher, idempotency);

        var envelope = BuildEnvelope(
            sanctionId: Guid.NewGuid(),
            subjectId: Guid.NewGuid(),
            enforcement: new SanctionEnforcementRefDto("Lock", Guid.NewGuid()),
            activatedAt: DateTimeOffset.UnixEpoch);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(envelope));

        // Second attempt — claim should have been released by the catch
        // block inside the handler, so TryClaimAsync returns true again.
        await handler.HandleAsync(envelope);

        Assert.Equal(2, callCount);
        await dispatcher.Received(2).DispatchSystemAsync(
            Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>());
    }
}
