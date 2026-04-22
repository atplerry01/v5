using Whycespace.Domain.StructuralSystem.Humancapital.Stewardship;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Stewardship;

public sealed class StewardshipAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static StewardshipId NewId(string seed) =>
        new(IdGen.Generate($"StewardshipAggregateTests:{seed}:stewardship"));

    private static StewardshipDescriptor DefaultDescriptor() =>
        new("Resource Stewardship", "Financial");

    [Fact]
    public void Create_RaisesStewardshipCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = StewardshipAggregate.Create(id, descriptor);

        var evt = Assert.IsType<StewardshipCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.StewardshipId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = StewardshipAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var s1 = StewardshipAggregate.Create(id, DefaultDescriptor());
        var s2 = StewardshipAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((StewardshipCreatedEvent)s1.DomainEvents[0]).StewardshipId.Value,
            ((StewardshipCreatedEvent)s2.DomainEvents[0]).StewardshipId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesStewardshipState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new StewardshipCreatedEvent(id, descriptor)
        };

        var aggregate = (StewardshipAggregate)Activator.CreateInstance(typeof(StewardshipAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
