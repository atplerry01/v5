using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Spv;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Spv;

public sealed class SpvAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static SpvId NewId(string seed) =>
        new(IdGen.Generate($"SpvAggregateTests:{seed}:spv"));

    private static SpvDescriptor DefaultDescriptor() =>
        new(new ClusterRef(IdGen.Generate("SpvAggregateTests:cluster-ref")), "Alpha SPV", SpvType.Operating);

    [Fact]
    public void Create_RaisesSpvCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = SpvAggregate.Create(id, descriptor);

        var evt = Assert.IsType<SpvCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SpvId);
        Assert.Equal(descriptor.SpvName, evt.Descriptor.SpvName);
    }

    [Fact]
    public void Create_SetsStatusToCreated()
    {
        var aggregate = SpvAggregate.Create(NewId("Create_Status"), DefaultDescriptor());

        Assert.Equal(SpvStatus.Created, aggregate.Status);
    }

    [Fact]
    public void Activate_FromCreated_RaisesSpvActivatedEvent()
    {
        var aggregate = SpvAggregate.Create(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<SpvActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SpvStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_RaisesSpvSuspendedEvent()
    {
        var aggregate = SpvAggregate.Create(NewId("Suspend_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<SpvSuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SpvStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Close_FromSuspended_RaisesSpvClosedEvent()
    {
        var aggregate = SpvAggregate.Create(NewId("Close_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.Suspend();
        aggregate.ClearDomainEvents();

        aggregate.Close();

        Assert.IsType<SpvClosedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SpvStatus.Closed, aggregate.Status);
    }

    [Fact]
    public void Retire_FromActive_RaisesSpvRetiredEvent()
    {
        var aggregate = SpvAggregate.Create(NewId("Retire_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<SpvRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SpvStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Close_FromCreated_Throws()
    {
        var aggregate = SpvAggregate.Create(NewId("Close_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Close());
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var s1 = SpvAggregate.Create(id, DefaultDescriptor());
        var s2 = SpvAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((SpvCreatedEvent)s1.DomainEvents[0]).SpvId.Value,
            ((SpvCreatedEvent)s2.DomainEvents[0]).SpvId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesSpvState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new SpvCreatedEvent(id, descriptor),
            new SpvActivatedEvent(id)
        };

        var aggregate = (SpvAggregate)Activator.CreateInstance(typeof(SpvAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(SpvStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
