using Whycespace.Domain.StructuralSystem.Humancapital.Workforce;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Workforce;

public sealed class WorkforceAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static WorkforceId NewId(string seed) =>
        new(IdGen.Generate($"WorkforceAggregateTests:{seed}:workforce"));

    private static WorkforceDescriptor DefaultDescriptor() =>
        new("Core Workforce", "Permanent");

    [Fact]
    public void Create_RaisesWorkforceCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = WorkforceAggregate.Create(id, descriptor);

        var evt = Assert.IsType<WorkforceCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.WorkforceId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = WorkforceAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var w1 = WorkforceAggregate.Create(id, DefaultDescriptor());
        var w2 = WorkforceAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((WorkforceCreatedEvent)w1.DomainEvents[0]).WorkforceId.Value,
            ((WorkforceCreatedEvent)w2.DomainEvents[0]).WorkforceId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesWorkforceState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new WorkforceCreatedEvent(id, descriptor)
        };

        var aggregate = (WorkforceAggregate)Activator.CreateInstance(typeof(WorkforceAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
