using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Structure.TopologyDefinition;

public sealed class TopologyDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static TopologyDefinitionId NewId(string seed) =>
        new(IdGen.Generate($"TopologyDefinitionAggregateTests:{seed}:topology-definition"));

    private static TopologyDefinitionDescriptor DefaultDescriptor() =>
        new("Star Topology", "Distributed");

    [Fact]
    public void Create_RaisesTopologyDefinitionCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = TopologyDefinitionAggregate.Create(id, descriptor);

        var evt = Assert.IsType<TopologyDefinitionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TopologyDefinitionId);
        Assert.Equal(descriptor.DefinitionName, evt.Descriptor.DefinitionName);
        Assert.Equal(descriptor.DefinitionKind, evt.Descriptor.DefinitionKind);
    }

    [Fact]
    public void Create_SetsInitialStatus()
    {
        var aggregate = TopologyDefinitionAggregate.Create(NewId("Create_Status"), DefaultDescriptor());

        Assert.Equal(TopologyDefinitionStatus.Draft, aggregate.Status);
    }

    [Fact]
    public void Activate_FromCreated_RaisesTopologyDefinitionActivatedEvent()
    {
        var aggregate = TopologyDefinitionAggregate.Create(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<TopologyDefinitionActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TopologyDefinitionStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_RaisesTopologyDefinitionSuspendedEvent()
    {
        var aggregate = TopologyDefinitionAggregate.Create(NewId("Suspend_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<TopologyDefinitionSuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TopologyDefinitionStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Retire_FromActive_RaisesTopologyDefinitionRetiredEvent()
    {
        var aggregate = TopologyDefinitionAggregate.Create(NewId("Retire_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<TopologyDefinitionRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TopologyDefinitionStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromDraft_Throws()
    {
        var aggregate = TopologyDefinitionAggregate.Create(NewId("Suspend_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Suspend());
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var t1 = TopologyDefinitionAggregate.Create(id, DefaultDescriptor());
        var t2 = TopologyDefinitionAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((TopologyDefinitionCreatedEvent)t1.DomainEvents[0]).TopologyDefinitionId.Value,
            ((TopologyDefinitionCreatedEvent)t2.DomainEvents[0]).TopologyDefinitionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesTopologyDefinitionState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new TopologyDefinitionCreatedEvent(id, descriptor),
            new TopologyDefinitionActivatedEvent(id)
        };

        var aggregate = (TopologyDefinitionAggregate)Activator.CreateInstance(typeof(TopologyDefinitionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(TopologyDefinitionStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
