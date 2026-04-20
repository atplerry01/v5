using NSubstitute;
using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Xunit;

namespace Whycespace.Tests.Unit.Integration.OutboundEffect;

/// <summary>
/// R3.B.1 / R-OUT-EFF-SEAM-03 — the factory is the canonical construction site
/// for every outbound-effect lifecycle event and for the aggregate itself.
/// </summary>
public sealed class OutboundEffectLifecycleEventFactoryTests
{
    private static readonly Guid EffectGuid = Guid.Parse("00000000-0000-0000-0000-000000000042");

    private static OutboundEffectLifecycleEventFactory NewFactory() =>
        new(Substitute.For<IPayloadTypeRegistry>());

    private static OutboundEffectOptions TestOptions() => new()
    {
        ProviderId = "noop",
        DispatchTimeoutMs = 5_000,
        TotalBudgetMs = 30_000,
        AckTimeoutMs = 10_000,
        FinalityWindowMs = 60_000,
        MaxAttempts = 3,
    };

    [Fact]
    public void Start_produces_aggregate_in_scheduled_state()
    {
        var factory = NewFactory();
        var aggregate = factory.Start(
            EffectGuid, "noop", "notification.send",
            new OutboundIdempotencyKey("k"), payload: new object(),
            schedulerActorId: "user/42", TestOptions());

        Assert.Equal(OutboundEffectStatus.Scheduled, aggregate.Status);
        Assert.Single(aggregate.DomainEvents);
    }

    [Fact]
    public void Start_rejects_null_idempotency_key()
    {
        var factory = NewFactory();
        Assert.Throws<ArgumentNullException>(() =>
            factory.Start(EffectGuid, "noop", "notification.send",
                idempotencyKey: null!, payload: new object(),
                schedulerActorId: "user/42", TestOptions()));
    }

    [Fact]
    public void Start_rejects_null_payload()
    {
        var factory = NewFactory();
        Assert.Throws<ArgumentNullException>(() =>
            factory.Start(EffectGuid, "noop", "notification.send",
                new OutboundIdempotencyKey("k"), payload: null!,
                schedulerActorId: "user/42", TestOptions()));
    }

    [Fact]
    public void Acknowledged_refuses_empty_provider_operation_id()
    {
        var factory = NewFactory();
        var identity = new ProviderOperationIdentity(ProviderId: "noop", ProviderOperationId: "");

        Assert.Throws<ArgumentException>(() => factory.Acknowledged(EffectGuid, identity));
    }

    [Fact]
    public void Dispatched_event_carries_attempt_number_and_timestamps()
    {
        var factory = NewFactory();
        var started = DateTimeOffset.Parse("2026-04-20T12:00:00Z");
        var completed = DateTimeOffset.Parse("2026-04-20T12:00:01Z");

        var evt = factory.Dispatched(EffectGuid, 2, started, completed, "evidence-hash");

        Assert.Equal(2, evt.AttemptNumber);
        Assert.Equal(started, evt.DispatchStartedAt);
        Assert.Equal(completed, evt.DispatchCompletedAt);
        Assert.Equal("evidence-hash", evt.TransportEvidenceDigest);
        Assert.Equal(EffectGuid, evt.AggregateId.Value);
    }

    [Fact]
    public void RetryAttempted_carries_classification_and_backoff()
    {
        var factory = NewFactory();
        var next = DateTimeOffset.Parse("2026-04-20T12:00:10Z");

        var evt = factory.RetryAttempted(
            EffectGuid, 2, next, 5_000,
            OutboundAdapterClassification.Transient);

        Assert.Equal(2, evt.AttemptNumber);
        Assert.Equal(next, evt.NextAttemptAt);
        Assert.Equal(5_000, evt.BackoffMs);
        Assert.Equal("Transient", evt.PrecedingClassification);
    }

    [Fact]
    public void Finalized_outcome_is_canonical_enum_string()
    {
        var factory = NewFactory();
        var at = DateTimeOffset.Parse("2026-04-20T12:00:00Z");

        var evt = factory.Finalized(
            EffectGuid, OutboundFinalityOutcome.Succeeded, "evidence", at, "SynchronousAck");

        Assert.Equal("Succeeded", evt.FinalityOutcome);
        Assert.Equal("SynchronousAck", evt.FinalitySource);
    }
}
