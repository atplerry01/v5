using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Structure.TypeDefinition;

public sealed class TypeDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static TypeDefinitionId NewId(string seed) =>
        new(IdGen.Generate($"TypeDefinitionAggregateTests:{seed}:type-definition"));

    private static TypeDefinitionDescriptor DefaultDescriptor() =>
        new("Operating Cluster", "Structural");

    [Fact]
    public void Define_RaisesTypeDefinitionDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = TypeDefinitionAggregate.Define(id, descriptor);

        var evt = Assert.IsType<TypeDefinitionDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TypeDefinitionId);
        Assert.Equal(descriptor.TypeName, evt.Descriptor.TypeName);
        Assert.Equal(descriptor.TypeCategory, evt.Descriptor.TypeCategory);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = TypeDefinitionAggregate.Define(NewId("Define_Status"), DefaultDescriptor());

        Assert.Equal(TypeDefinitionStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Activate_FromDefined_RaisesTypeDefinitionActivatedEvent()
    {
        var aggregate = TypeDefinitionAggregate.Define(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<TypeDefinitionActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TypeDefinitionStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Retire_FromActive_RaisesTypeDefinitionRetiredEvent()
    {
        var aggregate = TypeDefinitionAggregate.Define(NewId("Retire_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<TypeDefinitionRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TypeDefinitionStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Retire_FromDefined_Throws()
    {
        var aggregate = TypeDefinitionAggregate.Define(NewId("Retire_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Retire());
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var t1 = TypeDefinitionAggregate.Define(id, DefaultDescriptor());
        var t2 = TypeDefinitionAggregate.Define(id, DefaultDescriptor());

        Assert.Equal(
            ((TypeDefinitionDefinedEvent)t1.DomainEvents[0]).TypeDefinitionId.Value,
            ((TypeDefinitionDefinedEvent)t2.DomainEvents[0]).TypeDefinitionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesTypeDefinitionState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new TypeDefinitionDefinedEvent(id, descriptor),
            new TypeDefinitionActivatedEvent(id)
        };

        var aggregate = (TypeDefinitionAggregate)Activator.CreateInstance(typeof(TypeDefinitionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(TypeDefinitionStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
