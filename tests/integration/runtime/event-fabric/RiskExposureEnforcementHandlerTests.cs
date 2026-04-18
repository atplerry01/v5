using NSubstitute;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.Runtime.EventFabric;

/// <summary>
/// Phase 6 T6.5 — pins the risk → enforcement bridge. Every
/// <c>ExposureBreachedEvent</c> envelope observed by the handler MUST
/// produce exactly one <see cref="DetectViolationCommand"/> dispatched
/// on the economic enforcement violation route. Non-breach envelopes
/// MUST NOT dispatch.
/// </summary>
public sealed class RiskExposureEnforcementHandlerTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static IEventEnvelope BreachEnvelope(Guid exposureId) => new RawTestEnvelope
    {
        EventId = IdGen.Generate($"breach-envelope:{exposureId}"),
        AggregateId = exposureId,
        CorrelationId = IdGen.Generate($"breach-corr:{exposureId}"),
        EventType = "ExposureBreachedEvent",
        Timestamp = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero),
    };

    private static IEventEnvelope UnrelatedEnvelope() => new RawTestEnvelope
    {
        EventId = IdGen.Generate("unrelated-envelope"),
        AggregateId = IdGen.Generate("unrelated-agg"),
        EventType = "ExposureIncreasedEvent",
    };

    [Fact]
    public async Task HandleAsync_OnBreachEvent_DispatchesDetectViolationCommand()
    {
        var exposureId = IdGen.Generate("breach:exposure");
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var handler = new RiskExposureEnforcementHandler(dispatcher, IdGen, new TestClock());

        await handler.HandleAsync(BreachEnvelope(exposureId));

        await dispatcher.Received(1).DispatchAsync(
            Arg.Is<DetectViolationCommand>(c =>
                c.SourceReference == exposureId
                && c.Severity == "Critical"
                && c.RecommendedAction == "Restrict"),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "enforcement"
                && r.Domain == "violation"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_OnNonBreachEvent_DispatchesNothing()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        var handler = new RiskExposureEnforcementHandler(dispatcher, IdGen, new TestClock());

        await handler.HandleAsync(UnrelatedEnvelope());

        await dispatcher.DidNotReceiveWithAnyArgs()
            .DispatchAsync(default!, default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SameBreachEnvelope_ProducesDeterministicViolationId()
    {
        var exposureId = IdGen.Generate("deterministic:exposure");
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

        await handler.HandleAsync(envelope);
        await handler.HandleAsync(envelope);

        Assert.Equal(2, captured.Count);
        Assert.Equal(captured[0], captured[1]);
    }

    [Fact]
    public async Task HandleAsync_WhenDispatchFails_Throws()
    {
        var exposureId = IdGen.Generate("fail:exposure");
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Failure("policy_denied")));

        var handler = new RiskExposureEnforcementHandler(dispatcher, IdGen, new TestClock());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(BreachEnvelope(exposureId)));
    }
}
