using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Cluster;

public sealed class ClusterAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static ClusterId NewId(string seed) =>
        new(IdGen.Generate($"ClusterAggregateTests:{seed}:cluster"));

    private static ClusterDescriptor DefaultDescriptor() =>
        new("Test Cluster", "Primary");

    [Fact]
    public void Define_RaisesClusterDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = ClusterAggregate.Define(id, descriptor);

        var evt = Assert.IsType<ClusterDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ClusterId);
        Assert.Equal(descriptor.ClusterName, evt.Descriptor.ClusterName);
        Assert.Equal(descriptor.ClusterType, evt.Descriptor.ClusterType);
    }

    [Fact]
    public void Define_SetsInitialStatusToDefined()
    {
        var aggregate = ClusterAggregate.Define(NewId("Define_Status"), DefaultDescriptor());

        Assert.Equal(ClusterStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var c1 = ClusterAggregate.Define(id, DefaultDescriptor());
        var c2 = ClusterAggregate.Define(id, DefaultDescriptor());

        var e1 = (ClusterDefinedEvent)c1.DomainEvents[0];
        var e2 = (ClusterDefinedEvent)c2.DomainEvents[0];

        Assert.Equal(e1.ClusterId.Value, e2.ClusterId.Value);
    }

    [Fact]
    public void Activate_FromDefined_RaisesClusterActivatedEvent()
    {
        var aggregate = ClusterAggregate.Define(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        var evt = Assert.IsType<ClusterActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(aggregate.ClusterId, evt.ClusterId);
        Assert.Equal(ClusterStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Archive_FromActive_RaisesClusterArchivedEvent()
    {
        var aggregate = ClusterAggregate.Define(NewId("Archive_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Archive();

        var evt = Assert.IsType<ClusterArchivedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(aggregate.ClusterId, evt.ClusterId);
        Assert.Equal(ClusterStatus.Archived, aggregate.Status);
    }

    [Fact]
    public void Archive_FromDefined_Throws()
    {
        var aggregate = ClusterAggregate.Define(NewId("Archive_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Archive());
    }

    [Fact]
    public void RecordAuthorityAttached_AddsToActiveAuthorities()
    {
        var aggregate = ClusterAggregate.Define(NewId("Authority_Attach"), DefaultDescriptor());
        var authId = new ClusterAuthorityRef(IdGen.Generate("ClusterAggregateTests:Authority_Attach:auth"));

        aggregate.RecordAuthorityAttached(authId);

        Assert.Contains(authId, aggregate.ActiveAuthorities);
        Assert.IsType<ClusterAuthorityBoundEvent>(aggregate.DomainEvents.Last());
    }

    [Fact]
    public void RecordAuthorityAttached_Duplicate_Throws()
    {
        var aggregate = ClusterAggregate.Define(NewId("Authority_Dup"), DefaultDescriptor());
        var authId = new ClusterAuthorityRef(IdGen.Generate("ClusterAggregateTests:Authority_Dup:auth"));

        aggregate.RecordAuthorityAttached(authId);

        Assert.ThrowsAny<DomainException>(() => aggregate.RecordAuthorityAttached(authId));
    }

    [Fact]
    public void LoadFromHistory_RehydratesClusterState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new ClusterDefinedEvent(id, descriptor),
            new ClusterActivatedEvent(id)
        };

        var aggregate = (ClusterAggregate)Activator.CreateInstance(typeof(ClusterAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ClusterId);
        Assert.Equal(ClusterStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
