using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Structure.HierarchyDefinition;

public sealed class HierarchyDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static HierarchyDefinitionId NewId(string seed) =>
        new(IdGen.Generate($"HierarchyDefinitionAggregateTests:{seed}:hierarchy-definition"));

    private static HierarchyDefinitionDescriptor DefaultDescriptor() =>
        new("Cluster Hierarchy", IdGen.Generate("HierarchyDefinitionAggregateTests:parent-ref"));

    [Fact]
    public void Define_RaisesHierarchyDefinitionDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = HierarchyDefinitionAggregate.Define(id, descriptor);

        var evt = Assert.IsType<HierarchyDefinitionDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.HierarchyDefinitionId);
        Assert.Equal(descriptor.HierarchyName, evt.Descriptor.HierarchyName);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = HierarchyDefinitionAggregate.Define(NewId("Define_Status"), DefaultDescriptor());

        Assert.Equal(HierarchyDefinitionStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Validate_FromDefined_RaisesHierarchyDefinitionValidatedEvent()
    {
        var aggregate = HierarchyDefinitionAggregate.Define(NewId("Validate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Validate();

        Assert.IsType<HierarchyDefinitionValidatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(HierarchyDefinitionStatus.Validated, aggregate.Status);
    }

    [Fact]
    public void Lock_FromValidated_RaisesHierarchyDefinitionLockedEvent()
    {
        var aggregate = HierarchyDefinitionAggregate.Define(NewId("Lock_Valid"), DefaultDescriptor());
        aggregate.Validate();
        aggregate.ClearDomainEvents();

        aggregate.Lock();

        Assert.IsType<HierarchyDefinitionLockedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(HierarchyDefinitionStatus.Locked, aggregate.Status);
    }

    [Fact]
    public void Lock_FromDefined_Throws()
    {
        var aggregate = HierarchyDefinitionAggregate.Define(NewId("Lock_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Lock());
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var h1 = HierarchyDefinitionAggregate.Define(id, DefaultDescriptor());
        var h2 = HierarchyDefinitionAggregate.Define(id, DefaultDescriptor());

        Assert.Equal(
            ((HierarchyDefinitionDefinedEvent)h1.DomainEvents[0]).HierarchyDefinitionId.Value,
            ((HierarchyDefinitionDefinedEvent)h2.DomainEvents[0]).HierarchyDefinitionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesHierarchyDefinitionState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new HierarchyDefinitionDefinedEvent(id, descriptor),
            new HierarchyDefinitionValidatedEvent(id),
            new HierarchyDefinitionLockedEvent(id)
        };

        var aggregate = (HierarchyDefinitionAggregate)Activator.CreateInstance(typeof(HierarchyDefinitionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(HierarchyDefinitionStatus.Locked, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
