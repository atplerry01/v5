using Whycespace.Domain.StructuralSystem.Humancapital.Operator;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Operator;

public sealed class OperatorAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static OperatorId NewId(string seed) =>
        new(IdGen.Generate($"OperatorAggregateTests:{seed}:operator"));

    private static OperatorDescriptor DefaultDescriptor() =>
        new("Alice Operator", "ClusterAdmin");

    [Fact]
    public void Create_RaisesOperatorCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = OperatorAggregate.Create(id, descriptor);

        var evt = Assert.IsType<OperatorCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.OperatorId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = OperatorAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var o1 = OperatorAggregate.Create(id, DefaultDescriptor());
        var o2 = OperatorAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((OperatorCreatedEvent)o1.DomainEvents[0]).OperatorId.Value,
            ((OperatorCreatedEvent)o2.DomainEvents[0]).OperatorId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesOperatorState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new OperatorCreatedEvent(id, descriptor)
        };

        var aggregate = (OperatorAggregate)Activator.CreateInstance(typeof(OperatorAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
