using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.IntegrationSystem;

public sealed class OutboundEffectAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static Guid NewEffectId(string seed) =>
        IdGen.Generate($"OutboundEffectTests:{seed}:effect");

    private static OutboundEffectAggregate StartEffect(string seed) =>
        OutboundEffectAggregate.Start(
            NewEffectId(seed),
            "provider-stripe",
            "payment.charge",
            $"idempotency-{seed}",
            null,
            "scheduler-actor-001",
            dispatchTimeoutMs: 5000,
            totalBudgetMs: 30000,
            ackTimeoutMs: 10000,
            finalityWindowMs: 20000,
            maxAttempts: 3);

    [Fact]
    public void Start_RaisesOutboundEffectScheduledEvent()
    {
        var effectId = NewEffectId("Start_Valid");

        var aggregate = OutboundEffectAggregate.Start(
            effectId,
            "provider-email",
            "email.send",
            "key-abc-123",
            "EmailPayload",
            "scheduler-001",
            5000, 30000, 10000, 20000, 3);

        var evt = Assert.IsType<OutboundEffectScheduledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("provider-email", evt.ProviderId);
        Assert.Equal("email.send", evt.EffectType);
        Assert.Equal(3, evt.MaxAttempts);
    }

    [Fact]
    public void Start_SetsStatusToScheduled()
    {
        var aggregate = StartEffect("Status");

        Assert.Equal(OutboundEffectStatus.Scheduled, aggregate.Status);
        Assert.Equal("provider-stripe", aggregate.ProviderId);
        Assert.Equal("payment.charge", aggregate.EffectType);
    }

    [Fact]
    public void Start_WithSameSeed_ProducesStableIdentity()
    {
        var effectId = NewEffectId("Stable");

        var a1 = OutboundEffectAggregate.Start(effectId, "p", "t", "k", null, "s", 1, 1, 1, 1, 1);
        var a2 = OutboundEffectAggregate.Start(effectId, "p", "t", "k", null, "s", 1, 1, 1, 1, 1);

        Assert.Equal(
            ((OutboundEffectScheduledEvent)a1.DomainEvents[0]).AggregateId.Value,
            ((OutboundEffectScheduledEvent)a2.DomainEvents[0]).AggregateId.Value);
    }

    [Fact]
    public void Start_EmptyProviderId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            OutboundEffectAggregate.Start(NewEffectId("Empty_Provider"), "", "type", "key", null, "actor", 1, 1, 1, 1, 1));
    }

    [Fact]
    public void Start_EmptyEffectType_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            OutboundEffectAggregate.Start(NewEffectId("Empty_Type"), "provider", "", "key", null, "actor", 1, 1, 1, 1, 1));
    }

    [Fact]
    public void Start_EmptyIdempotencyKey_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            OutboundEffectAggregate.Start(NewEffectId("Empty_Key"), "provider", "type", "", null, "actor", 1, 1, 1, 1, 1));
    }

    [Fact]
    public void LoadFromHistory_RehydratesScheduledState()
    {
        var effectId = NewEffectId("History");
        var aggregateId = new AggregateId(effectId);

        var history = new object[]
        {
            new OutboundEffectScheduledEvent(
                aggregateId,
                "provider-kafka",
                "message.publish",
                "idempotency-history-key",
                null,
                "scheduler-002",
                5000, 30000, 10000, 20000, 5)
        };

        var aggregate = (OutboundEffectAggregate)Activator.CreateInstance(typeof(OutboundEffectAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(OutboundEffectStatus.Scheduled, aggregate.Status);
        Assert.Equal("provider-kafka", aggregate.ProviderId);
        Assert.Equal("message.publish", aggregate.EffectType);
        Assert.Equal(5, aggregate.MaxAttempts);
        Assert.Empty(aggregate.DomainEvents);
    }
}
