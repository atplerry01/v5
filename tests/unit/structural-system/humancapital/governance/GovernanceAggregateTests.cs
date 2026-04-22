using Whycespace.Domain.StructuralSystem.Humancapital.Governance;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Governance;

public sealed class GovernanceAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static GovernanceId NewId(string seed) =>
        new(IdGen.Generate($"GovernanceAggregateTests:{seed}:governance"));

    private static GovernanceDescriptor DefaultDescriptor() =>
        new("Standard Governance", "ClusterPolicy");

    [Fact]
    public void Create_RaisesGovernanceCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = GovernanceAggregate.Create(id, descriptor);

        var evt = Assert.IsType<GovernanceCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.GovernanceId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = GovernanceAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var g1 = GovernanceAggregate.Create(id, DefaultDescriptor());
        var g2 = GovernanceAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((GovernanceCreatedEvent)g1.DomainEvents[0]).GovernanceId.Value,
            ((GovernanceCreatedEvent)g2.DomainEvents[0]).GovernanceId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesGovernanceState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new GovernanceCreatedEvent(id, descriptor)
        };

        var aggregate = (GovernanceAggregate)Activator.CreateInstance(typeof(GovernanceAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
