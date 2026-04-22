using Whycespace.Domain.StructuralSystem.Humancapital.Performance;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Performance;

public sealed class PerformanceAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static PerformanceId NewId(string seed) =>
        new(IdGen.Generate($"PerformanceAggregateTests:{seed}:performance"));

    private static PerformanceDescriptor DefaultDescriptor() =>
        new("Q1 Performance", "Quarterly");

    [Fact]
    public void Create_RaisesPerformanceCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = PerformanceAggregate.Create(id, descriptor);

        var evt = Assert.IsType<PerformanceCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.PerformanceId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = PerformanceAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var p1 = PerformanceAggregate.Create(id, DefaultDescriptor());
        var p2 = PerformanceAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((PerformanceCreatedEvent)p1.DomainEvents[0]).PerformanceId.Value,
            ((PerformanceCreatedEvent)p2.DomainEvents[0]).PerformanceId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesPerformanceState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new PerformanceCreatedEvent(id, descriptor)
        };

        var aggregate = (PerformanceAggregate)Activator.CreateInstance(typeof(PerformanceAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
