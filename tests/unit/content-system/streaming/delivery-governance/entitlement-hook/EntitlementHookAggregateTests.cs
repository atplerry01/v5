using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class EntitlementHookAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static EntitlementHookId NewId(string seed) =>
        new(IdGen.Generate($"EntitlementHookAggregateTests:{seed}:hook"));

    [Fact]
    public void Register_RaisesEntitlementHookRegisteredEvent()
    {
        var id = NewId("Register_Valid");
        var targetRef = new EntitlementTargetRef(IdGen.Generate("EntitlementHookAggregateTests:target-ref"));
        var sourceRef = new SourceSystemRef("stripe");

        var aggregate = EntitlementHookAggregate.Register(id, targetRef, sourceRef, BaseTime);

        var evt = Assert.IsType<EntitlementHookRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.HookId);
        Assert.Equal(targetRef, evt.TargetRef);
        Assert.Equal(sourceRef, evt.SourceSystem);
    }

    [Fact]
    public void Register_SetsStateFromEvent()
    {
        var id = NewId("Register_State");
        var targetRef = new EntitlementTargetRef(IdGen.Generate("EntitlementHookAggregateTests:target-state"));
        var sourceRef = new SourceSystemRef("rights-mgr");

        var aggregate = EntitlementHookAggregate.Register(id, targetRef, sourceRef, BaseTime);

        Assert.Equal(id, aggregate.HookId);
        Assert.Equal(EntitlementStatus.Unknown, aggregate.Status);
    }

    [Fact]
    public void Register_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var targetRef = new EntitlementTargetRef(IdGen.Generate("EntitlementHookAggregateTests:stable-target"));
        var sourceRef = new SourceSystemRef("stripe");
        var h1 = EntitlementHookAggregate.Register(id, targetRef, sourceRef, BaseTime);
        var h2 = EntitlementHookAggregate.Register(id, targetRef, sourceRef, BaseTime);

        Assert.Equal(
            ((EntitlementHookRegisteredEvent)h1.DomainEvents[0]).HookId.Value,
            ((EntitlementHookRegisteredEvent)h2.DomainEvents[0]).HookId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesEntitlementHookState()
    {
        var id = NewId("History");
        var targetRef = new EntitlementTargetRef(IdGen.Generate("EntitlementHookAggregateTests:history-target"));
        var sourceRef = new SourceSystemRef("rights-mgr");

        var history = new object[]
        {
            new EntitlementHookRegisteredEvent(id, targetRef, sourceRef, BaseTime)
        };

        var aggregate = (EntitlementHookAggregate)Activator.CreateInstance(typeof(EntitlementHookAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.HookId);
        Assert.Equal(EntitlementStatus.Unknown, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
