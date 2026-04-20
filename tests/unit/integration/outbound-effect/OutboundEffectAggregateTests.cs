using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Xunit;

namespace Whycespace.Tests.Unit.Integration.OutboundEffect;

/// <summary>
/// R3.B.1 — aggregate-level invariants for <see cref="OutboundEffectAggregate"/>.
/// Validates state-machine discipline per design §5.4 and ratified constraints
/// #1 (Acknowledged ≠ Finalized) and #2 (ProviderOperationIdentity first-class).
///
/// The aggregate exposes no public mutator methods other than <c>Start</c>
/// (per <c>R-OUT-EFF-SEAM-01</c>); tests exercise post-construction transitions
/// via <see cref="AggregateRoot.LoadFromHistory"/>, which routes the event
/// stream through <c>Apply</c> — the same path used on runtime replay.
/// </summary>
public sealed class OutboundEffectAggregateTests
{
    private static readonly Guid EffectGuid = Guid.Parse("00000000-0000-0000-0000-000000000042");
    private static readonly AggregateId EffectAggregateId = new(EffectGuid);
    private static readonly DateTimeOffset DispatchStart = DateTimeOffset.Parse("2026-04-20T12:00:00Z");
    private static readonly DateTimeOffset DispatchEnd = DateTimeOffset.Parse("2026-04-20T12:00:01Z");

    private static OutboundEffectAggregate NewScheduled() =>
        OutboundEffectAggregate.Start(
            EffectGuid,
            providerId: "noop",
            effectType: "notification.send",
            idempotencyKey: "Payout:42:notification.send",
            payloadTypeDiscriminator: null,
            schedulerActorId: "user/42",
            dispatchTimeoutMs: 5_000,
            totalBudgetMs: 30_000,
            ackTimeoutMs: 10_000,
            finalityWindowMs: 60_000,
            maxAttempts: 3);

    private static OutboundEffectAggregate NewEmpty()
    {
        // Reflection into the private parameterless ctor — mirrors how the
        // runtime's aggregate-reconstruction loader bootstraps an empty
        // aggregate before LoadFromHistory. Parallels the test helper used
        // in WorkflowExecutionAggregate tests.
        var type = typeof(OutboundEffectAggregate);
        var ctor = type.GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);
        return (OutboundEffectAggregate)ctor!.Invoke(null);
    }

    private static OutboundEffectScheduledEvent ScheduledFixture() =>
        new(EffectAggregateId, "noop", "notification.send", "k", null, "a", 1, 1, 1, 1, 3);

    [Fact]
    public void Start_emits_scheduled_event_and_lands_status_scheduled()
    {
        var aggregate = NewScheduled();

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<OutboundEffectScheduledEvent>(aggregate.DomainEvents[0]);
        Assert.Equal(OutboundEffectStatus.Scheduled, aggregate.Status);
        Assert.Equal("noop", aggregate.ProviderId);
        Assert.Equal("notification.send", aggregate.EffectType);
        Assert.Equal(3, aggregate.MaxAttempts);
    }

    [Fact]
    public void Start_rejects_empty_provider_id()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            OutboundEffectAggregate.Start(
                EffectGuid, providerId: "",
                effectType: "x", idempotencyKey: "k", payloadTypeDiscriminator: null,
                schedulerActorId: "a", dispatchTimeoutMs: 1, totalBudgetMs: 1, ackTimeoutMs: 1,
                finalityWindowMs: 1, maxAttempts: 1));
    }

    [Fact]
    public void Start_rejects_empty_idempotency_key()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            OutboundEffectAggregate.Start(
                EffectGuid, providerId: "noop",
                effectType: "x", idempotencyKey: "  ", payloadTypeDiscriminator: null,
                schedulerActorId: "a", dispatchTimeoutMs: 1, totalBudgetMs: 1, ackTimeoutMs: 1,
                finalityWindowMs: 1, maxAttempts: 1));
    }

    [Fact]
    public void Replay_scheduled_dispatched_acknowledged_lands_acknowledged_with_op_id()
    {
        var replayed = NewEmpty();
        replayed.LoadFromHistory(new object[]
        {
            ScheduledFixture(),
            new OutboundEffectDispatchedEvent(EffectAggregateId, 1, DispatchStart, DispatchEnd),
            new OutboundEffectAcknowledgedEvent(EffectAggregateId, "noop", "op-abc"),
        });

        Assert.Equal(OutboundEffectStatus.Acknowledged, replayed.Status);
        Assert.Equal("op-abc", replayed.ProviderOperationId);
        Assert.Equal(1, replayed.AttemptCount);
    }

    [Fact]
    public void Replay_rejects_acknowledged_without_provider_operation_id()
    {
        var replayed = NewEmpty();

        Assert.Throws<DomainInvariantViolationException>(() =>
            replayed.LoadFromHistory(new object[]
            {
                ScheduledFixture(),
                new OutboundEffectDispatchedEvent(EffectAggregateId, 1, DispatchStart, DispatchEnd),
                new OutboundEffectAcknowledgedEvent(EffectAggregateId, ProviderId: "", ProviderOperationId: ""),
            }));
    }

    [Fact]
    public void Replay_rejects_acknowledged_without_dispatched()
    {
        var replayed = NewEmpty();

        Assert.Throws<DomainInvariantViolationException>(() =>
            replayed.LoadFromHistory(new object[]
            {
                ScheduledFixture(),
                new OutboundEffectAcknowledgedEvent(EffectAggregateId, "noop", "op-abc"),
            }));
    }

    [Fact]
    public void Replay_rejects_cancelled_after_dispatch()
    {
        var replayed = NewEmpty();

        Assert.Throws<DomainInvariantViolationException>(() =>
            replayed.LoadFromHistory(new object[]
            {
                ScheduledFixture(),
                new OutboundEffectDispatchedEvent(EffectAggregateId, 1, DispatchStart, DispatchEnd),
                new OutboundEffectCancelledEvent(EffectAggregateId, "too_late", PreDispatch: false),
            }));
    }

    [Fact]
    public void Replay_transient_failed_then_retry_returns_to_scheduled_status()
    {
        var replayed = NewEmpty();
        replayed.LoadFromHistory(new object[]
        {
            ScheduledFixture(),
            new OutboundEffectDispatchFailedEvent(EffectAggregateId, 1, "Transient", "timeout"),
            new OutboundEffectRetryAttemptedEvent(
                EffectAggregateId, 1, DispatchEnd.AddSeconds(5), 5000, "Transient"),
        });

        Assert.Equal(OutboundEffectStatus.Scheduled, replayed.Status);
        Assert.Equal("Transient", replayed.FailureClassification);
    }

    [Fact]
    public void Replay_retry_exhausted_is_terminal_for_further_retry_attempts()
    {
        var replayed = NewEmpty();

        Assert.Throws<DomainInvariantViolationException>(() =>
            replayed.LoadFromHistory(new object[]
            {
                ScheduledFixture(),
                new OutboundEffectRetryExhaustedEvent(EffectAggregateId, 3, "Transient"),
                new OutboundEffectRetryAttemptedEvent(
                    EffectAggregateId, 4, DispatchEnd, 1000, "Transient"),
            }));
    }

    [Fact]
    public void Replay_reconciled_requires_prior_reconciliation_required()
    {
        var replayed = NewEmpty();

        Assert.Throws<DomainInvariantViolationException>(() =>
            replayed.LoadFromHistory(new object[]
            {
                ScheduledFixture(),
                new OutboundEffectReconciledEvent(
                    EffectAggregateId, "Succeeded", "evidence-hash", "ops-user"),
            }));
    }

    [Fact]
    public void Replay_ack_timeout_path_lands_reconciliation_required_then_reconciled()
    {
        var replayed = NewEmpty();
        replayed.LoadFromHistory(new object[]
        {
            ScheduledFixture(),
            new OutboundEffectDispatchedEvent(EffectAggregateId, 1, DispatchStart, DispatchEnd),
            new OutboundEffectReconciliationRequiredEvent(
                EffectAggregateId, "AckTimeoutExpired", DispatchEnd.AddMinutes(5)),
            new OutboundEffectReconciledEvent(
                EffectAggregateId, "ManualIntervention", "evidence", "ops-user"),
        });

        Assert.Equal(OutboundEffectStatus.Reconciled, replayed.Status);
        Assert.Equal("ManualIntervention", replayed.FinalityOutcome);
    }
}
