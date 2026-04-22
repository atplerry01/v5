using Whycespace.Domain.StructuralSystem.Humancapital.Sanction;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Sanction;

public sealed class SanctionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static SanctionId NewId(string seed) =>
        new(IdGen.Generate($"SanctionAggregateTests:{seed}:sanction"));

    private static SanctionDescriptor DefaultDescriptor() =>
        new("Code Violation", "Disciplinary");

    [Fact]
    public void Create_RaisesSanctionCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = SanctionAggregate.Create(id, descriptor);

        var evt = Assert.IsType<SanctionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SanctionId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = SanctionAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var s1 = SanctionAggregate.Create(id, DefaultDescriptor());
        var s2 = SanctionAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((SanctionCreatedEvent)s1.DomainEvents[0]).SanctionId.Value,
            ((SanctionCreatedEvent)s2.DomainEvents[0]).SanctionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesSanctionState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new SanctionCreatedEvent(id, descriptor)
        };

        var aggregate = (SanctionAggregate)Activator.CreateInstance(typeof(SanctionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
