using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.3 — replay-equivalence for the outbound-effect aggregate. Two
/// replays of the same event stream MUST land in the same terminal status
/// and carry identical projected fields. This is the aggregate-side
/// invariant that backs future certification-grade replay-equivalence
/// harnesses (R5 scope).
/// </summary>
public sealed class OutboundEffectAggregateReplayEquivalenceTests
{
    private static readonly AggregateId AggregateId =
        new(Guid.Parse("66666666-0000-0000-0000-000000000001"));
    private static readonly DateTimeOffset T0 = DateTimeOffset.Parse("2026-04-20T12:00:00Z");

    private static OutboundEffectScheduledEvent ScheduledEvent() =>
        new(AggregateId, "p", "e", "k", null, "a", 100, 1_000, 500, 10_000, 3);

    private static OutboundEffectAggregate Replay(IEnumerable<object> history)
    {
        var ctor = typeof(OutboundEffectAggregate).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null, types: Type.EmptyTypes, modifiers: null);
        var aggregate = (OutboundEffectAggregate)ctor!.Invoke(null);
        aggregate.LoadFromHistory(history);
        return aggregate;
    }

    public static IEnumerable<object[]> Streams()
    {
        // Stream 1: ack path
        yield return new object[]
        {
            new object[]
            {
                ScheduledEvent(),
                new OutboundEffectDispatchedEvent(AggregateId, 1, T0, T0.AddSeconds(1)),
                new OutboundEffectAcknowledgedEvent(AggregateId, "p", "op-ack"),
            },
            OutboundEffectStatus.Acknowledged,
        };

        // Stream 2: transient then retry then acknowledged
        yield return new object[]
        {
            new object[]
            {
                ScheduledEvent(),
                new OutboundEffectDispatchFailedEvent(AggregateId, 1, "Transient", "timeout"),
                new OutboundEffectRetryAttemptedEvent(AggregateId, 1, T0.AddSeconds(2), 2_000, "Transient"),
                new OutboundEffectDispatchedEvent(AggregateId, 2, T0.AddSeconds(3), T0.AddSeconds(4)),
                new OutboundEffectAcknowledgedEvent(AggregateId, "p", "op-after-retry"),
            },
            OutboundEffectStatus.Acknowledged,
        };

        // Stream 3: retry exhausted
        yield return new object[]
        {
            new object[]
            {
                ScheduledEvent(),
                new OutboundEffectDispatchFailedEvent(AggregateId, 1, "Transient", "x"),
                new OutboundEffectDispatchFailedEvent(AggregateId, 2, "Transient", "x"),
                new OutboundEffectDispatchFailedEvent(AggregateId, 3, "Transient", "x"),
                new OutboundEffectRetryExhaustedEvent(AggregateId, 3, "Transient"),
            },
            OutboundEffectStatus.RetryExhausted,
        };

        // Stream 4: reconciliation-required → reconciled
        yield return new object[]
        {
            new object[]
            {
                ScheduledEvent(),
                new OutboundEffectDispatchedEvent(AggregateId, 1, T0, T0.AddSeconds(1)),
                new OutboundEffectReconciliationRequiredEvent(AggregateId, "DispatchAmbiguous", T0.AddSeconds(2)),
                new OutboundEffectReconciledEvent(AggregateId, "Succeeded", "digest", "ops-user"),
            },
            OutboundEffectStatus.Reconciled,
        };

        // Stream 5: cancellation pre-dispatch
        yield return new object[]
        {
            new object[]
            {
                ScheduledEvent(),
                new OutboundEffectCancelledEvent(AggregateId, "operator_abort", true),
            },
            OutboundEffectStatus.Cancelled,
        };
    }

    [Theory]
    [MemberData(nameof(Streams))]
    public void Two_replays_of_the_same_stream_land_identical_status(
        object[] events, OutboundEffectStatus expected)
    {
        var a = Replay(events);
        var b = Replay(events);

        Assert.Equal(expected, a.Status);
        Assert.Equal(a.Status, b.Status);
        Assert.Equal(a.ProviderOperationId, b.ProviderOperationId);
        Assert.Equal(a.FailureClassification, b.FailureClassification);
        Assert.Equal(a.FinalityOutcome, b.FinalityOutcome);
        Assert.Equal(a.ReconciliationCause, b.ReconciliationCause);
        Assert.Equal(a.AttemptCount, b.AttemptCount);
    }
}
