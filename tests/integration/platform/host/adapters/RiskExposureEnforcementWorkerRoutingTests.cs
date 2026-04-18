using NSubstitute;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.Platform.Host.Adapters;

/// <summary>
/// Phase 6 Final Patch (F2, F3) — pins the end-to-end behaviour of the
/// risk-exposure event bridge at the handler boundary:
///
///   Kafka envelope → EventDeserializer → RiskExposureEnforcementHandler
///   → ISystemIntentDispatcher (DetectViolationCommand)
///
/// The <see cref="Whycespace.Platform.Host.Adapters.RiskExposureEnforcementWorker"/>
/// is a thin Kafka-plumbing wrapper around the handler — its
/// deserialization path is the same one exercised by
/// <c>ReconciliationLifecycleWorker</c> and
/// <c>EnforcementDetectionWorker</c> and does not warrant its own
/// integration harness. These tests drive the handler directly with
/// envelopes identical to what the worker would emit, so the contract
/// is pinned without requiring a live Kafka broker.
///
/// Covers:
///   * F2 — breach envelope → DetectViolationCommand is dispatched on
///     the economic enforcement violation route.
///   * F3 — a handler-level dispatch failure surfaces as a thrown
///     exception so the worker's catch-and-do-not-commit path is
///     reached (message is redelivered, not dropped).
///   * Non-breach envelopes on the same topic MUST NOT produce any
///     enforcement traffic (guards against phantom violations).
/// </summary>
public sealed class RiskExposureEnforcementWorkerRoutingTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static IEventEnvelope BreachEnvelope(Guid exposureId) => new RawTestEnvelope
    {
        EventId = IdGen.Generate($"F2-breach:{exposureId}"),
        AggregateId = exposureId,
        CorrelationId = IdGen.Generate($"F2-corr:{exposureId}"),
        EventType = "ExposureBreachedEvent",
        Timestamp = TestClock.Frozen,
    };

    [Fact]
    public async Task Breach_Envelope_DispatchesDetectViolationOnViolationRoute()
    {
        var exposureId = IdGen.Generate("F2:exposure");
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var handler = new RiskExposureEnforcementHandler(dispatcher, IdGen, new TestClock());

        await handler.HandleAsync(BreachEnvelope(exposureId));

        await dispatcher.Received(1).DispatchAsync(
            Arg.Is<DetectViolationCommand>(c => c.SourceReference == exposureId),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "enforcement"
                && r.Domain == "violation"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("ExposureCreatedEvent")]
    [InlineData("ExposureIncreasedEvent")]
    [InlineData("ExposureReducedEvent")]
    [InlineData("ExposureClosedEvent")]
    public async Task NonBreach_Envelopes_DoNotDispatch(string eventType)
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        var handler = new RiskExposureEnforcementHandler(dispatcher, IdGen, new TestClock());

        var envelope = new RawTestEnvelope
        {
            EventId = IdGen.Generate($"F2-nonbreach:{eventType}"),
            AggregateId = IdGen.Generate($"F2-nonbreach-agg:{eventType}"),
            EventType = eventType,
            Timestamp = TestClock.Frozen,
        };

        await handler.HandleAsync(envelope);

        await dispatcher.DidNotReceiveWithAnyArgs()
            .DispatchAsync(default!, default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handler_DispatchFailure_ThrowsSoWorkerDoesNotCommitOffset()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Failure("downstream_unavailable")));

        var handler = new RiskExposureEnforcementHandler(dispatcher, IdGen, new TestClock());

        var envelope = BreachEnvelope(IdGen.Generate("F3:exposure"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(envelope));

        Assert.Contains("downstream_unavailable", ex.Message);
    }

    [Fact]
    public async Task Redelivered_BreachEnvelope_ProducesSameViolationId()
    {
        var exposureId = IdGen.Generate("F2-redelivery:exposure");
        var envelope = BreachEnvelope(exposureId);

        var captured = new List<Guid>();
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                if (ci.Arg<object>() is DetectViolationCommand v)
                    captured.Add(v.ViolationId);
                return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
            });

        var handler = new RiskExposureEnforcementHandler(dispatcher, IdGen, new TestClock());

        // Kafka will redeliver on uncommitted offset; the downstream
        // DetectViolation engine dedupes by ViolationId. We pin that
        // the id remains stable across redeliveries of the same envelope.
        await handler.HandleAsync(envelope);
        await handler.HandleAsync(envelope);
        await handler.HandleAsync(envelope);

        Assert.Equal(3, captured.Count);
        Assert.Single(captured.Distinct());
    }
}
