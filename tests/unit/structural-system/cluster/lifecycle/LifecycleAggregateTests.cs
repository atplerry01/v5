using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Lifecycle;

public sealed class LifecycleAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static LifecycleId NewId(string seed) =>
        new(IdGen.Generate($"LifecycleAggregateTests:{seed}:lifecycle"));

    private static LifecycleDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("LifecycleAggregateTests:cluster-ref"), "Alpha Lifecycle");

    [Fact]
    public void Define_RaisesLifecycleDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = LifecycleAggregate.Define(id, descriptor);

        var evt = Assert.IsType<LifecycleDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.LifecycleId);
        Assert.Equal(descriptor.LifecycleName, evt.Descriptor.LifecycleName);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = LifecycleAggregate.Define(NewId("Define_Status"), DefaultDescriptor());

        Assert.Equal(LifecycleStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Transition_FromDefined_RaisesLifecycleTransitionedEvent()
    {
        var aggregate = LifecycleAggregate.Define(NewId("Transition_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Transition();

        Assert.IsType<LifecycleTransitionedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(LifecycleStatus.Transitioned, aggregate.Status);
    }

    [Fact]
    public void Complete_FromTransitioned_RaisesLifecycleCompletedEvent()
    {
        var aggregate = LifecycleAggregate.Define(NewId("Complete_Valid"), DefaultDescriptor());
        aggregate.Transition();
        aggregate.ClearDomainEvents();

        aggregate.Complete();

        Assert.IsType<LifecycleCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(LifecycleStatus.Completed, aggregate.Status);
    }

    [Fact]
    public void Complete_FromDefined_Throws()
    {
        var aggregate = LifecycleAggregate.Define(NewId("Complete_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Complete());
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var l1 = LifecycleAggregate.Define(id, DefaultDescriptor());
        var l2 = LifecycleAggregate.Define(id, DefaultDescriptor());

        Assert.Equal(
            ((LifecycleDefinedEvent)l1.DomainEvents[0]).LifecycleId.Value,
            ((LifecycleDefinedEvent)l2.DomainEvents[0]).LifecycleId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesLifecycleState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new LifecycleDefinedEvent(id, descriptor),
            new LifecycleTransitionedEvent(id),
            new LifecycleCompletedEvent(id)
        };

        var aggregate = (LifecycleAggregate)Activator.CreateInstance(typeof(LifecycleAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(LifecycleStatus.Completed, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
