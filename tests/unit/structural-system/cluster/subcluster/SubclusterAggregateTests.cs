using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Subcluster;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Subcluster;

public sealed class SubclusterAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static SubclusterId NewId(string seed) =>
        new(IdGen.Generate($"SubclusterAggregateTests:{seed}:subcluster"));

    private static SubclusterDescriptor DefaultDescriptor() =>
        new(new ClusterRef(IdGen.Generate("SubclusterAggregateTests:parent-cluster-ref")), "Beta Subcluster");

    [Fact]
    public void Define_RaisesSubclusterDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = SubclusterAggregate.Define(id, descriptor);

        var evt = Assert.IsType<SubclusterDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SubclusterId);
        Assert.Equal(descriptor.SubclusterName, evt.Descriptor.SubclusterName);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = SubclusterAggregate.Define(NewId("Define_Status"), DefaultDescriptor());

        Assert.Equal(SubclusterStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Activate_FromDefined_RaisesSubclusterActivatedEvent()
    {
        var aggregate = SubclusterAggregate.Define(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<SubclusterActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SubclusterStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Archive_FromActive_RaisesSubclusterArchivedEvent()
    {
        var aggregate = SubclusterAggregate.Define(NewId("Archive_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Archive();

        Assert.IsType<SubclusterArchivedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SubclusterStatus.Archived, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_RaisesSubclusterSuspendedEvent()
    {
        var aggregate = SubclusterAggregate.Define(NewId("Suspend_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<SubclusterSuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SubclusterStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Archive_FromDefined_Throws()
    {
        var aggregate = SubclusterAggregate.Define(NewId("Archive_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Archive());
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var s1 = SubclusterAggregate.Define(id, DefaultDescriptor());
        var s2 = SubclusterAggregate.Define(id, DefaultDescriptor());

        Assert.Equal(
            ((SubclusterDefinedEvent)s1.DomainEvents[0]).SubclusterId.Value,
            ((SubclusterDefinedEvent)s2.DomainEvents[0]).SubclusterId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesSubclusterState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new SubclusterDefinedEvent(id, descriptor),
            new SubclusterActivatedEvent(id)
        };

        var aggregate = (SubclusterAggregate)Activator.CreateInstance(typeof(SubclusterAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(SubclusterStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
