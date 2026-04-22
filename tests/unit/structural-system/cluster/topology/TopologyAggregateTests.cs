using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Topology;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Topology;

public sealed class TopologyAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static TopologyId NewId(string seed) =>
        new(IdGen.Generate($"TopologyAggregateTests:{seed}:topology"));

    private static TopologyDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("TopologyAggregateTests:cluster-ref"), "Alpha Topology");

    [Fact]
    public void Define_RaisesTopologyDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = TopologyAggregate.Define(id, descriptor);

        var evt = Assert.IsType<TopologyDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TopologyId);
        Assert.Equal(descriptor.TopologyName, evt.Descriptor.TopologyName);
        Assert.Equal(descriptor.ClusterReference, evt.Descriptor.ClusterReference);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = TopologyAggregate.Define(NewId("Define_Status"), DefaultDescriptor());

        Assert.Equal(TopologyStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Validate_FromDefined_RaisesTopologyValidatedEvent()
    {
        var aggregate = TopologyAggregate.Define(NewId("Validate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Validate();

        Assert.IsType<TopologyValidatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TopologyStatus.Validated, aggregate.Status);
    }

    [Fact]
    public void Lock_FromValidated_RaisesTopologyLockedEvent()
    {
        var aggregate = TopologyAggregate.Define(NewId("Lock_Valid"), DefaultDescriptor());
        aggregate.Validate();
        aggregate.ClearDomainEvents();

        aggregate.Lock();

        Assert.IsType<TopologyLockedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TopologyStatus.Locked, aggregate.Status);
    }

    [Fact]
    public void Lock_FromDefined_Throws()
    {
        var aggregate = TopologyAggregate.Define(NewId("Lock_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Lock());
    }

    [Fact]
    public void LoadFromHistory_RehydratesTopologyState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new TopologyDefinedEvent(id, descriptor),
            new TopologyValidatedEvent(id),
            new TopologyLockedEvent(id)
        };

        var aggregate = (TopologyAggregate)Activator.CreateInstance(typeof(TopologyAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(TopologyStatus.Locked, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var t1 = TopologyAggregate.Define(id, DefaultDescriptor());
        var t2 = TopologyAggregate.Define(id, DefaultDescriptor());

        Assert.Equal(
            ((TopologyDefinedEvent)t1.DomainEvents[0]).TopologyId.Value,
            ((TopologyDefinedEvent)t2.DomainEvents[0]).TopologyId.Value);
    }
}
