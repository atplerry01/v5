using Whycespace.Domain.StructuralSystem.Humancapital.Incentive;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Incentive;

public sealed class IncentiveAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static IncentiveId NewId(string seed) =>
        new(IdGen.Generate($"IncentiveAggregateTests:{seed}:incentive"));

    private static IncentiveDescriptor DefaultDescriptor() =>
        new("Performance Incentive", "Monetary");

    [Fact]
    public void Create_RaisesIncentiveCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = IncentiveAggregate.Create(id, descriptor);

        var evt = Assert.IsType<IncentiveCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.IncentiveId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = IncentiveAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var i1 = IncentiveAggregate.Create(id, DefaultDescriptor());
        var i2 = IncentiveAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((IncentiveCreatedEvent)i1.DomainEvents[0]).IncentiveId.Value,
            ((IncentiveCreatedEvent)i2.DomainEvents[0]).IncentiveId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesIncentiveState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new IncentiveCreatedEvent(id, descriptor)
        };

        var aggregate = (IncentiveAggregate)Activator.CreateInstance(typeof(IncentiveAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
