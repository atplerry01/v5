using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Structure.Classification;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Structure.Classification;

public sealed class ClassificationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static ClassificationId NewId(string seed) =>
        new(IdGen.Generate($"ClassificationAggregateTests:{seed}:classification"));

    private static ClassificationDescriptor DefaultDescriptor() =>
        new("Cluster Tier", "Structural");

    [Fact]
    public void Define_RaisesClassificationDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = ClassificationAggregate.Define(id, descriptor);

        var evt = Assert.IsType<ClassificationDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ClassificationId);
        Assert.Equal(descriptor.ClassificationName, evt.Descriptor.ClassificationName);
        Assert.Equal(descriptor.ClassificationCategory, evt.Descriptor.ClassificationCategory);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = ClassificationAggregate.Define(NewId("Define_Status"), DefaultDescriptor());

        Assert.Equal(ClassificationStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Activate_FromDefined_RaisesClassificationActivatedEvent()
    {
        var aggregate = ClassificationAggregate.Define(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<ClassificationActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ClassificationStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Deprecate_FromActive_RaisesClassificationDeprecatedEvent()
    {
        var aggregate = ClassificationAggregate.Define(NewId("Deprecate_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<ClassificationDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Deprecate_FromDefined_Throws()
    {
        var aggregate = ClassificationAggregate.Define(NewId("Deprecate_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Deprecate());
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var c1 = ClassificationAggregate.Define(id, DefaultDescriptor());
        var c2 = ClassificationAggregate.Define(id, DefaultDescriptor());

        Assert.Equal(
            ((ClassificationDefinedEvent)c1.DomainEvents[0]).ClassificationId.Value,
            ((ClassificationDefinedEvent)c2.DomainEvents[0]).ClassificationId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesClassificationState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new ClassificationDefinedEvent(id, descriptor),
            new ClassificationActivatedEvent(id)
        };

        var aggregate = (ClassificationAggregate)Activator.CreateInstance(typeof(ClassificationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(ClassificationStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
